namespace WristMatch.Domain;

public sealed class GuestSession
{
    private GuestSession() { }

    public GuestSession(string tokenHash, DateTimeOffset now, TimeSpan lifetime)
    {
        Id = Guid.NewGuid();
        TokenHash = tokenHash;
        CreatedAt = now;
        ExpiresAt = now.Add(lifetime);
    }

    public Guid Id { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public List<PhotoAsset> Photos { get; private set; } = [];
}

public sealed class PhotoAsset
{
    private PhotoAsset() { }

    public PhotoAsset(Guid sessionId, string storageKey, string mimeType, int width, int height, long byteSize, string consentVersion, DateTimeOffset consentAt, DateTimeOffset expiresAt)
    {
        Id = Guid.NewGuid();
        SessionId = sessionId;
        StorageKey = storageKey;
        MimeType = mimeType;
        Width = width;
        Height = height;
        ByteSize = byteSize;
        ConsentVersion = consentVersion;
        ConsentAt = consentAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public GuestSession Session { get; private set; } = null!;
    public string StorageKey { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public long ByteSize { get; private set; }
    public string ConsentVersion { get; private set; } = string.Empty;
    public DateTimeOffset ConsentAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public void Delete() => DeletedAt = DateTimeOffset.UtcNow;
}
