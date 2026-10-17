using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WristMatch.Application;

namespace WristMatch.Infrastructure.YouCam;

public sealed class YouCamTryOnProvider(YouCamClient client, IConfiguration configuration, ILogger<YouCamTryOnProvider> logger) : ITryOnProvider
{
    public string Mode => "youcam";

    public async Task<TryOnProviderResult> GenerateAsync(TryOnProviderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var srcFileId = await client.UploadLocalFileAsync(request.WristPhotoPath, cancellationToken);
            var productIsPublicUrl = Uri.TryCreate(request.ProductImageUrl, UriKind.Absolute, out var productUri) &&
                (productUri.Scheme == Uri.UriSchemeHttp || productUri.Scheme == Uri.UriSchemeHttps);

            object payload;
            if (productIsPublicUrl)
            {
                payload = new
                {
                    src_file_id = srcFileId,
                    ref_file_urls = new[] { request.ProductImageUrl },
                    source_info = new { name = Path.GetFileName(request.WristPhotoPath) },
                    object_infos = new[]
                    {
                        new
                        {
                            name = request.ProductImageUrl,
                            parameter = new
                            {
                                watch_need_remove_background = true,
                                watch_anchor_point = (object?)null,
                                watch_wearing_location = (object?)null,
                                watch_shadow_intensity = configuration.GetValue("YouCam:WatchShadowIntensity", 0.3),
                                watch_ambient_light_intensity = configuration.GetValue("YouCam:WatchAmbientLightIntensity", 1.0),
                            },
                        },
                    },
                };
            }
            else
            {
                var refFileId = await client.UploadFromUrlOrPathAsync(request.ProductImageUrl, request.OutputDirectory, cancellationToken);
                payload = new
                {
                    src_file_id = srcFileId,
                    ref_file_ids = new[] { refFileId },
                    source_info = new { name = Path.GetFileName(request.WristPhotoPath) },
                    object_infos = new[]
                    {
                        new
                        {
                            name = request.ProductName,
                            parameter = new
                            {
                                watch_need_remove_background = true,
                                watch_anchor_point = (object?)null,
                                watch_wearing_location = (object?)null,
                                watch_shadow_intensity = configuration.GetValue("YouCam:WatchShadowIntensity", 0.3),
                                watch_ambient_light_intensity = configuration.GetValue("YouCam:WatchAmbientLightIntensity", 1.0),
                            },
                        },
                    },
                };
            }

            var taskId = await client.CreateTaskAsync("/s2s/v2.0/task/2d-vto/watch", payload, cancellationToken);

            var data = await client.PollTaskAsync("/s2s/v2.0/task/2d-vto/watch", taskId, cancellationToken);
            var resultUrl = YouCamClient.ExtractResultUrl(data)
                ?? ExtractNestedResultUrl(data)
                ?? throw new YouCamException("YouCam Watch VTO succeeded but returned no result URL.");

            var relativeKey = Path.Combine("tryon-results", DateTimeOffset.UtcNow.ToString("yyyyMMdd"), $"{request.JobId:N}.jpg");
            var absolute = Path.Combine(request.OutputDirectory, relativeKey);
            await client.DownloadToFileAsync(resultUrl, absolute, cancellationToken);

            return new TryOnProviderResult(true, relativeKey.Replace('\\', '/'), taskId, $"YouCam Watch VTO preview for {request.ProductName}");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "YouCam Watch VTO failed for job {JobId}", request.JobId);
            var message = exception is YouCamException youCam
                ? youCam.Message
                : "YouCam Watch VTO failed. Check the wrist photo and try again.";
            return new TryOnProviderResult(false, null, $"youcam-error-{request.JobId:N}"[..24], message);
        }
    }

    private static string? ExtractNestedResultUrl(JsonElement data)
    {
        if (!data.TryGetProperty("results", out var results))
            return null;
        if (results.ValueKind != JsonValueKind.Object)
            return null;
        foreach (var property in results.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String &&
                property.Name.Contains("url", StringComparison.OrdinalIgnoreCase))
                return property.Value.GetString();
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String && item.GetString()?.StartsWith("http", StringComparison.OrdinalIgnoreCase) == true)
                        return item.GetString();
                    if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("url", out var url))
                        return url.GetString();
                }
            }
        }
        return null;
    }
}
