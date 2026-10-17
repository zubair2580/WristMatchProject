namespace WristMatch.Domain;

public sealed class ProductAsset
{
    private ProductAsset() { }

    public ProductAsset(Guid productId, string storageKey, string mimeType, int width, int height, long byteSize, Guid uploadedByUserId, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid(); ProductId = productId; StorageKey = storageKey; MimeType = mimeType; Width = width; Height = height; ByteSize = byteSize; UploadedByUserId = uploadedByUserId; CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public string StorageKey { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public long ByteSize { get; private set; }
    public bool IsApproved { get; private set; }
    public bool IsPrimary { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public ProductAssetLicence? Licence { get; private set; }

    public void Approve(bool primary) { IsApproved = true; IsPrimary = primary; }
    public void RemovePrimary() => IsPrimary = false;
}

public sealed class ProductAssetLicence
{
    private ProductAssetLicence() { }

    public ProductAssetLicence(Guid productAssetId, string source, string licenceNote, string? attribution, Guid approvedByUserId, DateTimeOffset approvedAt)
    {
        Id = Guid.NewGuid(); ProductAssetId = productAssetId; Source = source; LicenceNote = licenceNote; Attribution = attribution; ApprovedByUserId = approvedByUserId; ApprovedAt = approvedAt;
    }

    public Guid Id { get; private set; }
    public Guid ProductAssetId { get; private set; }
    public ProductAsset ProductAsset { get; private set; } = null!;
    public string Source { get; private set; } = string.Empty;
    public string LicenceNote { get; private set; } = string.Empty;
    public string? Attribution { get; private set; }
    public Guid ApprovedByUserId { get; private set; }
    public DateTimeOffset ApprovedAt { get; private set; }
}
