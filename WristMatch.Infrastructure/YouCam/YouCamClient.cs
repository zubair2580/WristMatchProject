using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WristMatch.Infrastructure.YouCam;

public sealed class YouCamClient(HttpClient http, IConfiguration configuration, ILogger<YouCamClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    private string ApiKey => configuration["YouCam:ApiKey"] ?? string.Empty;

    private string BaseUrl => (configuration["YouCam:BaseUrl"] ?? "https://yce-api-01.makeupar.com").TrimEnd('/');

    private int PollIntervalMs => configuration.GetValue("YouCam:PollIntervalMs", 2500);

    private int PollTimeoutSeconds => configuration.GetValue("YouCam:PollTimeoutSeconds", 120);

    public async Task<string> UploadFileAsync(byte[] bytes, string fileName, string contentType, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var normalizedType = NormalizeContentType(contentType);
        using var initRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/s2s/v2.0/file");
        ApplyAuth(initRequest);
        initRequest.Content = JsonContent.Create(new
        {
            files = new[]
            {
                new { content_type = normalizedType, file_name = fileName, file_size = bytes.Length },
            },
        });

        using var initResponse = await http.SendAsync(initRequest, cancellationToken);
        var initBody = await initResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!initResponse.IsSuccessStatusCode)
            throw new YouCamException($"File API failed ({(int)initResponse.StatusCode}): {Trim(initBody)}");

        using var initDoc = JsonDocument.Parse(initBody);
        var fileNode = initDoc.RootElement.GetProperty("data").GetProperty("files")[0];
        var fileId = fileNode.GetProperty("file_id").GetString()
            ?? throw new YouCamException("File API did not return file_id.");
        var upload = fileNode.GetProperty("requests")[0];
        var uploadUrl = upload.GetProperty("url").GetString()
            ?? throw new YouCamException("File API did not return upload URL.");
        var method = upload.TryGetProperty("method", out var methodNode) ? methodNode.GetString() ?? "PUT" : "PUT";

        using var uploadRequest = new HttpRequestMessage(new HttpMethod(method), uploadUrl);
        uploadRequest.Content = new ByteArrayContent(bytes);
        uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(normalizedType);
        uploadRequest.Content.Headers.ContentLength = bytes.Length;
        if (upload.TryGetProperty("headers", out var headers) && headers.ValueKind == JsonValueKind.Object)
        {
            foreach (var header in headers.EnumerateObject())
            {
                if (header.NameEquals("Content-Type") || header.NameEquals("Content-Length"))
                    continue;
                uploadRequest.Headers.TryAddWithoutValidation(header.Name, header.Value.GetString());
            }
        }

        using var uploadResponse = await http.SendAsync(uploadRequest, cancellationToken);
        if (!uploadResponse.IsSuccessStatusCode)
        {
            var uploadBody = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new YouCamException($"Presigned upload failed ({(int)uploadResponse.StatusCode}): {Trim(uploadBody)}");
        }

        return fileId;
    }

    public async Task<string> UploadLocalFileAsync(string path, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        var extension = Path.GetExtension(path).ToLowerInvariant();
        var contentType = extension is ".png" ? "image/png" : "image/jpg";
        var fileName = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = $"upload{extension}";
        return await UploadFileAsync(bytes, fileName, contentType, cancellationToken);
    }

    public async Task<string> UploadFromUrlOrPathAsync(string imageUrlOrPath, string? localRoot, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imageUrlOrPath))
            throw new YouCamException("A product image is required for YouCam Watch VTO.");

        if (Uri.TryCreate(imageUrlOrPath, UriKind.Absolute, out var absolute) &&
            (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            using var response = await http.GetAsync(absolute, cancellationToken);
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? GuessContentType(imageUrlOrPath);
            var fileName = Path.GetFileName(absolute.LocalPath);
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('.'))
                fileName = contentType.Contains("png", StringComparison.OrdinalIgnoreCase) ? "product.png" : "product.jpg";
            return await UploadFileAsync(bytes, fileName, contentType, cancellationToken);
        }

        var relative = imageUrlOrPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(localRoot))
            candidates.Add(Path.Combine(localRoot, relative));
        candidates.Add(Path.Combine(AppContext.BaseDirectory, "wwwroot", relative));
        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relative));
        candidates.Add(Path.GetFullPath(relative));

        var localPath = candidates.FirstOrDefault(File.Exists)
            ?? throw new YouCamException($"Product image could not be resolved: {imageUrlOrPath}");
        return await UploadLocalFileAsync(localPath, cancellationToken);
    }

    public async Task<string> CreateTaskAsync(string relativePath, object payload, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}{relativePath}");
        ApplyAuth(request);
        request.Content = JsonContent.Create(payload, options: JsonOptions);
        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new YouCamException($"Create task failed ({(int)response.StatusCode}): {Trim(body)}");

        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("data", out var data))
            throw new YouCamException("Create task response missing data.");
        var taskId = data.TryGetProperty("task_id", out var idNode) ? idNode.GetString() : null;
        if (string.IsNullOrWhiteSpace(taskId))
            throw new YouCamException("Create task response missing task_id.");
        return taskId;
    }

    public async Task<JsonElement> PollTaskAsync(string relativePath, string taskId, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var deadline = DateTimeOffset.UtcNow.AddSeconds(PollTimeoutSeconds);
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}{relativePath.TrimEnd('/')}/{Uri.EscapeDataString(taskId)}");
            ApplyAuth(request);
            using var response = await http.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new YouCamException($"Poll task failed ({(int)response.StatusCode}): {Trim(body)}");

            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("data", out var data))
                throw new YouCamException("Poll response missing data.");

            var status = data.TryGetProperty("task_status", out var statusNode) ? statusNode.GetString() ?? string.Empty : string.Empty;
            if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
                return data.Clone();
            if (string.Equals(status, "error", StringComparison.OrdinalIgnoreCase))
            {
                var error = data.TryGetProperty("error", out var errorNode) ? errorNode.ToString() : "unknown";
                throw new YouCamException($"YouCam task failed: {error}");
            }

            logger.LogDebug("YouCam task {TaskId} status {Status}", taskId, status);
            await Task.Delay(PollIntervalMs, cancellationToken);
        }

        throw new YouCamException("YouCam task timed out before completion.");
    }

    public async Task DownloadToFileAsync(string url, string absolutePath, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        using var response = await http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var file = File.Create(absolutePath);
        await stream.CopyToAsync(file, cancellationToken);
    }

    public static string? ExtractResultUrl(JsonElement data)
    {
        if (!data.TryGetProperty("results", out var results))
            return null;

        if (results.ValueKind == JsonValueKind.Object)
        {
            if (results.TryGetProperty("url", out var urlNode))
                return urlNode.GetString();
            if (results.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array && output.GetArrayLength() > 0)
            {
                var first = output[0];
                if (first.TryGetProperty("url", out var nested))
                    return nested.GetString();
            }
        }

        if (results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
        {
            var first = results[0];
            if (first.ValueKind == JsonValueKind.String)
                return first.GetString();
            if (first.TryGetProperty("url", out var urlNode))
                return urlNode.GetString();
        }

        return null;
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
            throw new YouCamException("YouCam:ApiKey is not configured.");
    }

    private void ApplyAuth(HttpRequestMessage request) =>
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

    private static string NormalizeContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return "image/jpg";
        if (contentType.Contains("png", StringComparison.OrdinalIgnoreCase))
            return "image/png";
        if (contentType.Contains("jpeg", StringComparison.OrdinalIgnoreCase) || contentType.Contains("jpg", StringComparison.OrdinalIgnoreCase))
            return "image/jpg";
        return contentType;
    }

    private static string GuessContentType(string path) =>
        path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpg";

    private static string Trim(string value) => value.Length <= 400 ? value : value[..400] + "...";
}

public sealed class YouCamException(string message) : Exception(message);
