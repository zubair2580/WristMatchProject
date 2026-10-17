using WristMatch.Domain;

namespace WristMatch.Application;

public sealed record TryOnJobResult(Guid Id, Guid ProductId, string State, string ProviderMode, string? SafeMessage, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt, DateTimeOffset ExpiresAt, string? ResultUrl);
public sealed record TryOnCreateRequest(Guid PhotoId, Guid ProductId, string IdempotencyKey);

public interface ITryOnJobService
{
    Task<TryOnJobResult> CreateAsync(Guid sessionId, TryOnCreateRequest request, CancellationToken cancellationToken);
    Task<TryOnJobResult?> GetAsync(Guid sessionId, Guid jobId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid sessionId, Guid jobId, CancellationToken cancellationToken);
    Task<(TryOnJob Job, Stream Content)?> OpenResultAsync(Guid sessionId, Guid jobId, CancellationToken cancellationToken);
}

public interface ITryOnProvider
{
    string Mode { get; }
    Task<TryOnProviderResult> GenerateAsync(TryOnProviderRequest request, CancellationToken cancellationToken);
}

public sealed record TryOnProviderRequest(Guid JobId, Guid ProductId, string ProductName, string WristPhotoPath, string OutputDirectory, string ProductImageUrl);
public sealed record TryOnProviderResult(bool Succeeded, string? ResultStorageKey, string ProviderTaskId, string? SafeMessage);

public sealed class TryOnValidationException(string message, string code) : Exception(message)
{
    public string Code { get; } = code;
}
