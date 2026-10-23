using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WristMatch.Application;
using WristMatch.Domain;

namespace WristMatch.Infrastructure.YouCam;

public sealed class LookMatchService(
    WristMatchDbContext db,
    YouCamClient client,
    IPhotoAssetService photos,
    IConfiguration configuration,
    ILogger<LookMatchService> logger) : ILookMatchService
{
    public async Task<LookMatchResult> AnalyzeAsync(Guid sessionId, Stream facePhoto, string contentType, long byteSize, bool consent, string consentVersion, CancellationToken cancellationToken)
    {
        var upload = await photos.UploadAsync(new PhotoUploadRequest(sessionId, facePhoto, contentType, byteSize, consent, consentVersion), cancellationToken);
        var root = configuration["Storage:PrivateRoot"];
        var storageRoot = Path.IsPathRooted(root) ? root! : Path.Combine(AppContext.BaseDirectory, root ?? "private-media");
        var photo = await db.PhotoAssets.AsNoTracking().SingleAsync(item => item.Id == upload.Id, cancellationToken);
        var photoPath = Path.GetFullPath(Path.Combine(storageRoot, photo.StorageKey));

        var undertone = await EstimateUndertoneAsync(photoPath, cancellationToken);
        var scores = new List<LookMatchScore>();
        var providerMode = "demo";

        if (client.IsConfigured)
        {
            try
            {
                providerMode = "youcam";
                var fileId = await client.UploadLocalFileAsync(photoPath, cancellationToken);
                var taskId = await client.CreateTaskAsync("/s2s/v2.0/task/skin-analysis", new
                {
                    src_file_id = fileId,
                    dst_actions = new[] { "wrinkle", "pore", "texture", "acne" },
                    format = "json",
                }, cancellationToken);
                var data = await client.PollTaskAsync("/s2s/v2.0/task/skin-analysis", taskId, cancellationToken);
                scores.AddRange(ParseScores(data));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "YouCam Skin Analysis failed; continuing with local undertone match.");
                providerMode = "youcam-fallback";
            }
        }

        var preferredMetals = PreferredMetalsFor(undertone);
        var preferredColours = PreferredColoursFor(undertone);
        var products = await db.Products.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.Name)
            .Take(40)
            .ToListAsync(cancellationToken);

        var recommendations = products
            .Select(product => Rank(product, preferredMetals, preferredColours, undertone, scores))
            .OrderByDescending(item => item.MatchScore)
            .ThenBy(item => item.Price)
            .Take(6)
            .ToList();

        var summary = scores.Count > 0
            ? $"Skin analysis mapped a {undertone} undertone profile and ranked {recommendations.Count} watches by metal and colour harmony."
            : $"Undertone estimate ({undertone}) ranked {recommendations.Count} watches for metal and colour harmony.";

        return new LookMatchResult(providerMode, undertone, preferredMetals, preferredColours, scores, recommendations, summary);
    }

    private static IEnumerable<LookMatchScore> ParseScores(JsonElement data)
    {
        if (!data.TryGetProperty("results", out var results))
            yield break;

        JsonElement output = default;
        if (results.ValueKind == JsonValueKind.Object && results.TryGetProperty("output", out var nested))
            output = nested;
        else if (results.ValueKind == JsonValueKind.Array)
            output = results;
        else
            yield break;

        if (output.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var item in output.EnumerateArray())
        {
            var concern = item.TryGetProperty("type", out var type) ? type.GetString() ?? "concern" : "concern";
            var region = item.TryGetProperty("region", out var regionNode) ? regionNode.GetString() ?? "whole" : "whole";
            double? raw = item.TryGetProperty("raw_score", out var rawNode) && rawNode.TryGetDouble(out var rawValue) ? rawValue : null;
            int? ui = item.TryGetProperty("ui_score", out var uiNode) && uiNode.TryGetInt32(out var uiValue) ? uiValue : null;
            double? score = item.TryGetProperty("score", out var scoreNode) && scoreNode.TryGetDouble(out var scoreValue) ? scoreValue : null;
            yield return new LookMatchScore(concern, region, raw, ui, score);
        }
    }

    private static async Task<string> EstimateUndertoneAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        using var image = await Image.LoadAsync<Rgba32>(stream, cancellationToken);
        image.Mutate(ctx => ctx.Resize(new ResizeOptions { Size = new Size(64, 64), Mode = ResizeMode.Crop }));

        long r = 0, g = 0, b = 0, count = 0;
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];
                    if (pixel.A < 200) continue;
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3;
                    if (brightness is < 40 or > 230) continue;
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                    count++;
                }
            }
        });

        if (count == 0)
            return "neutral";

        var avgR = r / (double)count;
        var avgG = g / (double)count;
        var avgB = b / (double)count;
        if (avgR - avgB > 18 && avgR > avgG)
            return "warm";
        if (avgB - avgR > 10)
            return "cool";
        return "neutral";
    }

    private static IReadOnlyList<string> PreferredMetalsFor(string undertone) => undertone switch
    {
        "warm" => ["Yellow Gold", "Rose Gold", "Bronze", "Leather"],
        "cool" => ["Silver", "Stainless Steel", "White Gold", "Titanium"],
        _ => ["Stainless Steel", "Rose Gold", "Silver", "Leather"],
    };

    private static IReadOnlyList<string> PreferredColoursFor(string undertone) => undertone switch
    {
        "warm" => ["Gold", "Rose", "Brown", "Champagne", "Copper"],
        "cool" => ["Silver", "Blue", "Black", "Grey", "White"],
        _ => ["Silver", "Gold", "Black", "Blue", "Brown"],
    };

    private static LookMatchProduct Rank(Product product, IReadOnlyList<string> metals, IReadOnlyList<string> colours, string undertone, IReadOnlyList<LookMatchScore> scores)
    {
        var haystack = $"{product.Colour} {product.Material}".ToLowerInvariant();
        var score = 40;
        var reasons = new List<string>();

        foreach (var metal in metals)
        {
            if (!haystack.Contains(metal.ToLowerInvariant().Split(' ')[0])) continue;
            score += 18;
            reasons.Add($"{metal} suits {undertone} undertones");
            break;
        }

        foreach (var colour in colours)
        {
            if (!haystack.Contains(colour.ToLowerInvariant())) continue;
            score += 12;
            reasons.Add($"{colour} complements your look profile");
            break;
        }

        var texture = scores.FirstOrDefault(item => item.Concern.Contains("texture", StringComparison.OrdinalIgnoreCase));
        if (texture?.UiScore is >= 70 && product.Material.Contains("Steel", StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
            reasons.Add("Clean steel finish pairs with balanced skin texture");
        }

        var acne = scores.FirstOrDefault(item => item.Concern.Contains("acne", StringComparison.OrdinalIgnoreCase));
        if (acne?.UiScore is < 50 && product.Colour.Contains("Rose", StringComparison.OrdinalIgnoreCase))
        {
            score += 6;
            reasons.Add("Softer rose metal reads gently against skin contrast");
        }

        if (reasons.Count == 0)
            reasons.Add($"Neutral catalogue pick for {undertone} undertone shopping");

        return new LookMatchProduct(
            product.Id,
            product.Name,
            product.Slug,
            product.Colour,
            product.Material,
            product.ImageUrl,
            product.Price,
            product.Currency,
            Math.Clamp(score, 1, 99),
            string.Join(" · ", reasons.Take(2)));
    }
}
