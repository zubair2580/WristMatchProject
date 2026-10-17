using WristMatch.Domain;

namespace WristMatch.Application;

public sealed record GuestSessionResult(Guid Id, DateTimeOffset ExpiresAt, bool Created);
public sealed record PhotoUploadRequest(Guid SessionId, Stream Content, string? DeclaredContentType, long Length, bool Consent, string ConsentVersion);
public sealed record PhotoAssetResult(Guid Id, DateTimeOffset ExpiresAt, string PreviewUrl, int Width, int Height, long ByteSize);

public interface IGuestSessionService
{
    Task<(GuestSession Session, string RawToken, bool Created)> GetOrCreateAsync(string? rawToken, CancellationToken cancellationToken);
    Task<GuestSession?> FindAsync(string rawToken, CancellationToken cancellationToken);
    Task DeleteAsync(GuestSession session, CancellationToken cancellationToken);
}

public interface IPhotoAssetService
{
    Task<PhotoAssetResult> UploadAsync(PhotoUploadRequest request, CancellationToken cancellationToken);
    Task<(PhotoAsset Asset, Stream Content)?> OpenAsync(Guid sessionId, Guid photoId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid sessionId, Guid photoId, CancellationToken cancellationToken);
}

public sealed class PhotoValidationException(string message, string code) : Exception(message)
{
    public string Code { get; } = code;
}
