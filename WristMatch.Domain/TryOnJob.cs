namespace WristMatch.Domain;

public enum TryOnJobState
{
    Queued = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Expired = 5
}

public sealed class TryOnJob
{
    private TryOnJob() { }

    public TryOnJob(Guid sessionId, Guid photoId, Guid productId, string idempotencyKey, string requestHash, string providerMode, DateTimeOffset now, TimeSpan lifetime)
    {
        Id = Guid.NewGuid();
        SessionId = sessionId;
        PhotoId = photoId;
        ProductId = productId;
        IdempotencyKey = idempotencyKey;
        RequestHash = requestHash;
        ProviderMode = providerMode;
        State = TryOnJobState.Queued;
        CreatedAt = now;
        UpdatedAt = now;
        ExpiresAt = now.Add(lifetime);
    }

    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid PhotoId { get; private set; }
    public Guid ProductId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public TryOnJobState State { get; private set; }
    public string ProviderMode { get; private set; } = "demo";
    public string? ProviderTaskId { get; private set; }
    public string? ResultStorageKey { get; private set; }
    public string? SafeMessage { get; private set; }
    public DateTimeOffset? WorkerLeaseUntil { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    public bool TryAcquireLease(DateTimeOffset now, TimeSpan lease)
    {
        if (State is not TryOnJobState.Queued and not TryOnJobState.Processing)
            return false;
        if (State == TryOnJobState.Processing && WorkerLeaseUntil is DateTimeOffset until && until > now)
            return false;
        State = TryOnJobState.Processing;
        WorkerLeaseUntil = now.Add(lease);
        UpdatedAt = now;
        return true;
    }

    public void MarkSucceeded(string resultStorageKey, string providerTaskId, DateTimeOffset now)
    {
        State = TryOnJobState.Succeeded;
        ResultStorageKey = resultStorageKey;
        ProviderTaskId = providerTaskId;
        SafeMessage = "Preview ready";
        CompletedAt = now;
        WorkerLeaseUntil = null;
        UpdatedAt = now;
    }

    public void MarkFailed(string safeMessage, DateTimeOffset now)
    {
        State = TryOnJobState.Failed;
        SafeMessage = safeMessage;
        CompletedAt = now;
        WorkerLeaseUntil = null;
        UpdatedAt = now;
    }

    public void MarkExpired(DateTimeOffset now)
    {
        State = TryOnJobState.Expired;
        SafeMessage = "This preview has expired.";
        CompletedAt ??= now;
        WorkerLeaseUntil = null;
        UpdatedAt = now;
    }

    public void SoftDelete(DateTimeOffset now)
    {
        ResultStorageKey = null;
        SafeMessage = "Preview deleted";
        UpdatedAt = now;
        if (State == TryOnJobState.Succeeded)
            State = TryOnJobState.Expired;
        CompletedAt ??= now;
    }
}
