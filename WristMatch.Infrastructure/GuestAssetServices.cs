using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using WristMatch.Application;
using WristMatch.Domain;

namespace WristMatch.Infrastructure;

public sealed class GuestSessionService(WristMatchDbContext db, IConfiguration configuration) : IGuestSessionService
{
    private const string CookieTokenLength = "SessionTokenLength";

    public async Task<(GuestSession Session, string RawToken, bool Created)> GetOrCreateAsync(string? rawToken, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(rawToken))
        {
            var existing = await FindAsync(rawToken, cancellationToken);
            if (existing is not null)
                return (existing, rawToken, false);
        }

        var tokenBytes = RandomNumberGenerator.GetBytes(configuration.GetValue(CookieTokenLength, 32));
        var token = Convert.ToBase64String(tokenBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var now = DateTimeOffset.UtcNow;
        var session = new GuestSession(Hash(token), now, TimeSpan.FromHours(configuration.GetValue("Privacy:SessionHours", 24)));
        db.GuestSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return (session, token, true);
    }

    public async Task<GuestSession?> FindAsync(string rawToken, CancellationToken cancellationToken)
    {
        var hash = Hash(rawToken);
        return await db.GuestSessions.SingleOrDefaultAsync(session => session.TokenHash == hash && session.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);
    }

    public async Task DeleteAsync(GuestSession session, CancellationToken cancellationToken)
    {
        var photos = await db.PhotoAssets.Where(photo => photo.SessionId == session.Id && photo.DeletedAt == null).ToListAsync(cancellationToken);
        foreach (var photo in photos)
            photo.Delete();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}

public sealed class PhotoAssetService(WristMatchDbContext db, IConfiguration configuration) : IPhotoAssetService
{
    public async Task<PhotoAssetResult> UploadAsync(PhotoUploadRequest request, CancellationToken cancellationToken)
    {
        if (!request.Consent)
            throw new PhotoValidationException("Consent is required before uploading a photo.", "CONSENT_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.ConsentVersion))
            throw new PhotoValidationException("A consent version is required.", "CONSENT_VERSION_REQUIRED");
        if (request.Length is <= 0 or > 8 * 1024 * 1024)
            throw new PhotoValidationException("Photos must be JPEG or PNG files no larger than 8 MiB.", "PHOTO_SIZE_INVALID");

        using var input = new MemoryStream();
        await request.Content.CopyToAsync(input, cancellationToken);
        input.Position = 0;
        try
        {
            var format = await Image.DetectFormatAsync(input, cancellationToken);
            if (format.Name is not "JPEG" and not "PNG")
                throw new PhotoValidationException("Photos must be JPEG or PNG files.", "PHOTO_FORMAT_INVALID");
            input.Position = 0;
            using var image = await Image.LoadAsync(input, cancellationToken);
            if (image.Width > 4096 || image.Height > 4096 || (long)image.Width * image.Height > configuration.GetValue("Privacy:MaxDecodedPixels", 16_000_000))
                throw new PhotoValidationException("The photo dimensions are not supported.", "PHOTO_DIMENSIONS_INVALID");

            var root = configuration["Storage:PrivateRoot"];
            var storageRoot = Path.IsPathRooted(root) ? root : Path.Combine(AppContext.BaseDirectory, root ?? "private-media");
            Directory.CreateDirectory(storageRoot);
            var storageKey = Path.Combine(DateTimeOffset.UtcNow.ToString("yyyyMMdd"), $"{Guid.NewGuid():N}.jpg");
            var absolutePath = Path.Combine(storageRoot, storageKey);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;
            await image.SaveAsJpegAsync(absolutePath, new JpegEncoder { Quality = 92 }, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var asset = new PhotoAsset(request.SessionId, storageKey, "image/jpeg", image.Width, image.Height, new FileInfo(absolutePath).Length, request.ConsentVersion, now, now.AddHours(configuration.GetValue("Privacy:AssetHours", 24)));
            db.PhotoAssets.Add(asset);
            await db.SaveChangesAsync(cancellationToken);
            return new PhotoAssetResult(asset.Id, asset.ExpiresAt, $"/api/v1/photos/{asset.Id}/image", asset.Width, asset.Height, asset.ByteSize);
        }
        catch (UnknownImageFormatException)
        {
            throw new PhotoValidationException("Photos must be valid JPEG or PNG files.", "PHOTO_FORMAT_INVALID");
        }
        catch (InvalidImageContentException)
        {
            throw new PhotoValidationException("The photo content is corrupt or unreadable.", "PHOTO_CONTENT_INVALID");
        }
    }

    public async Task<(PhotoAsset Asset, Stream Content)?> OpenAsync(Guid sessionId, Guid photoId, CancellationToken cancellationToken)
    {
        var asset = await db.PhotoAssets.SingleOrDefaultAsync(photo => photo.Id == photoId && photo.SessionId == sessionId && photo.DeletedAt == null && photo.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);
        if (asset is null)
            return null;
        var root = configuration["Storage:PrivateRoot"];
        var storageRoot = Path.IsPathRooted(root) ? root : Path.Combine(AppContext.BaseDirectory, root ?? "private-media");
        var fullPath = Path.GetFullPath(Path.Combine(storageRoot, asset.StorageKey));
        if (!fullPath.StartsWith(Path.GetFullPath(storageRoot), StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
            return null;
        return (asset, File.OpenRead(fullPath));
    }

    public async Task<bool> DeleteAsync(Guid sessionId, Guid photoId, CancellationToken cancellationToken)
    {
        var asset = await db.PhotoAssets.SingleOrDefaultAsync(photo => photo.Id == photoId && photo.SessionId == sessionId && photo.DeletedAt == null, cancellationToken);
        if (asset is null)
            return false;
        asset.Delete();
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
