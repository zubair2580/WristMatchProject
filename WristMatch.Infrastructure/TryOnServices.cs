using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using WristMatch.Application;
using WristMatch.Domain;

namespace WristMatch.Infrastructure;

public sealed class TryOnJobService(WristMatchDbContext db, IConfiguration configuration, ITryOnProvider provider) : ITryOnJobService
{
    public async Task<TryOnJobResult> CreateAsync(Guid sessionId, TryOnCreateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Length > 120)
            throw new TryOnValidationException("Provide an Idempotency-Key of up to 120 characters.", "IDEMPOTENCY_KEY_INVALID");

        var existing = await db.TryOnJobs.AsNoTracking()
            .SingleOrDefaultAsync(job => job.SessionId == sessionId && job.IdempotencyKey == request.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            if (existing.RequestHash != HashRequest(sessionId, request.PhotoId, request.ProductId))
                throw new TryOnValidationException("This Idempotency-Key was already used with a different request.", "IDEMPOTENCY_KEY_CONFLICT");
            return ToResult(existing);
        }

        var photo = await db.PhotoAssets.AsNoTracking().SingleOrDefaultAsync(item => item.Id == request.PhotoId && item.SessionId == sessionId && item.DeletedAt == null && item.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);
        if (photo is null)
            throw new TryOnValidationException("Upload a private wrist photo before generating a preview.", "PHOTO_REQUIRED");

        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(item => item.Id == request.ProductId && item.IsActive, cancellationToken);
        if (product is null)
            throw new TryOnValidationException("Choose an active catalogue product for the preview.", "PRODUCT_REQUIRED");

        var now = DateTimeOffset.UtcNow;
        var job = new TryOnJob(sessionId, request.PhotoId, request.ProductId, request.IdempotencyKey.Trim(), HashRequest(sessionId, request.PhotoId, request.ProductId), provider.Mode, now, TimeSpan.FromHours(configuration.GetValue("Privacy:AssetHours", 24)));
        db.TryOnJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);
        return ToResult(job);
    }

    public async Task<TryOnJobResult?> GetAsync(Guid sessionId, Guid jobId, CancellationToken cancellationToken)
    {
        var job = await db.TryOnJobs.AsNoTracking().SingleOrDefaultAsync(item => item.Id == jobId && item.SessionId == sessionId, cancellationToken);
        return job is null ? null : ToResult(job);
    }

    public async Task<bool> DeleteAsync(Guid sessionId, Guid jobId, CancellationToken cancellationToken)
    {
        var job = await db.TryOnJobs.SingleOrDefaultAsync(item => item.Id == jobId && item.SessionId == sessionId, cancellationToken);
        if (job is null)
            return false;
        if (!string.IsNullOrWhiteSpace(job.ResultStorageKey))
        {
            var path = ResolvePath(job.ResultStorageKey);
            if (File.Exists(path))
                File.Delete(path);
        }
        job.SoftDelete(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(TryOnJob Job, Stream Content)?> OpenResultAsync(Guid sessionId, Guid jobId, CancellationToken cancellationToken)
    {
        var job = await db.TryOnJobs.AsNoTracking().SingleOrDefaultAsync(item => item.Id == jobId && item.SessionId == sessionId, cancellationToken);
        if (job is null || job.State != TryOnJobState.Succeeded || string.IsNullOrWhiteSpace(job.ResultStorageKey) || job.ExpiresAt <= DateTimeOffset.UtcNow)
            return null;
        var path = ResolvePath(job.ResultStorageKey);
        if (!File.Exists(path))
            return null;
        return (job, File.OpenRead(path));
    }

    private string ResolvePath(string storageKey)
    {
        var root = configuration["Storage:PrivateRoot"];
        var storageRoot = Path.IsPathRooted(root) ? root! : Path.Combine(AppContext.BaseDirectory, root ?? "private-media");
        var fullPath = Path.GetFullPath(Path.Combine(storageRoot, storageKey));
        if (!fullPath.StartsWith(Path.GetFullPath(storageRoot), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");
        return fullPath;
    }

    private static string HashRequest(Guid sessionId, Guid photoId, Guid productId) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{sessionId:N}:{photoId:N}:{productId:N}")));

    private static TryOnJobResult ToResult(TryOnJob job) => new(
        job.Id,
        job.ProductId,
        job.State.ToString(),
        job.ProviderMode,
        job.SafeMessage,
        job.CreatedAt,
        job.CompletedAt,
        job.ExpiresAt,
        job.State == TryOnJobState.Succeeded ? $"/api/v1/tryon-jobs/{job.Id}/result" : null);
}

public sealed class DemoTryOnProvider(IConfiguration configuration) : ITryOnProvider
{
    public string Mode => "demo";

    public async Task<TryOnProviderResult> GenerateAsync(TryOnProviderRequest request, CancellationToken cancellationToken)
    {
        _ = request.ProductImageUrl;
        await Task.Delay(configuration.GetValue("TryOn:DemoDelayMs", 1200), cancellationToken);
        Directory.CreateDirectory(request.OutputDirectory);
        var relativeKey = Path.Combine("tryon-results", DateTimeOffset.UtcNow.ToString("yyyyMMdd"), $"{request.JobId:N}.jpg");
        var absolute = Path.Combine(request.OutputDirectory, relativeKey);
        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

        using var image = await Image.LoadAsync(request.WristPhotoPath, cancellationToken);
        // Demo adapter: normalize the private photo into a labelled preview artifact without calling an external provider.
        image.Mutate(ctx => ctx.GaussianBlur(0.6f).Contrast(1.05f));
        await image.SaveAsJpegAsync(absolute, new JpegEncoder { Quality = 90 }, cancellationToken);
        return new TryOnProviderResult(true, relativeKey.Replace('\\', '/'), $"demo-{Guid.NewGuid():N}"[..24], $"Demo preview for {request.ProductName}");
    }
}

public sealed class TryOnWorker(IServiceScopeFactory scopeFactory, ILogger<TryOnWorker> logger, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessOnce(stoppingToken); }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Try-on worker cycle failed.");
            }
            await Task.Delay(configuration.GetValue("TryOn:PollMs", 2000), stoppingToken);
        }
    }

    private async Task ProcessOnce(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<WristMatchDbContext>();
        var provider = scope.ServiceProvider.GetRequiredService<ITryOnProvider>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var now = DateTimeOffset.UtcNow;

        var expired = await db.TryOnJobs.Where(job => job.ExpiresAt <= now && (job.State == TryOnJobState.Queued || job.State == TryOnJobState.Processing || job.State == TryOnJobState.Succeeded)).Take(20).ToListAsync(cancellationToken);
        foreach (var job in expired)
            job.MarkExpired(now);
        if (expired.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        var candidate = await db.TryOnJobs
            .Where(job => job.State == TryOnJobState.Queued || (job.State == TryOnJobState.Processing && (job.WorkerLeaseUntil == null || job.WorkerLeaseUntil < now)))
            .OrderBy(job => job.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (candidate is null || !candidate.TryAcquireLease(now, TimeSpan.FromSeconds(config.GetValue("TryOn:LeaseSeconds", 60))))
            return;
        await db.SaveChangesAsync(cancellationToken);

        var photo = await db.PhotoAssets.AsNoTracking().SingleOrDefaultAsync(item => item.Id == candidate.PhotoId && item.DeletedAt == null, cancellationToken);
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(item => item.Id == candidate.ProductId, cancellationToken);
        if (photo is null || product is null)
        {
            candidate.MarkFailed("The source photo or product is no longer available.", DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var root = config["Storage:PrivateRoot"];
        var storageRoot = Path.IsPathRooted(root) ? root! : Path.Combine(AppContext.BaseDirectory, root ?? "private-media");
        var photoPath = Path.GetFullPath(Path.Combine(storageRoot, photo.StorageKey));
        if (!File.Exists(photoPath))
        {
            candidate.MarkFailed("The private wrist photo could not be opened.", DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            var result = await provider.GenerateAsync(new TryOnProviderRequest(candidate.Id, product.Id, product.Name, photoPath, storageRoot, product.ImageUrl), cancellationToken);
            if (result.Succeeded && !string.IsNullOrWhiteSpace(result.ResultStorageKey))
                candidate.MarkSucceeded(result.ResultStorageKey, result.ProviderTaskId, DateTimeOffset.UtcNow);
            else
                candidate.MarkFailed(result.SafeMessage ?? "Preview generation failed.", DateTimeOffset.UtcNow);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Try-on generation failed for {JobId}", candidate.Id);
            candidate.MarkFailed("Preview generation failed. Try again with another photo.", DateTimeOffset.UtcNow);
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
