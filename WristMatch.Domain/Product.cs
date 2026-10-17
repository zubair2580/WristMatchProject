namespace WristMatch.Domain;

public enum ProductCategory
{
    Watch = 1,
    Bracelet = 2
}

public enum Occasion
{
    Office = 1,
    Wedding = 2,
    Casual = 3,
    Gift = 4
}

public sealed class Product
{
    private Product() { }

    public Product(string slug, string name, string description, decimal price, string currency, string colour, string material, string imageUrl, string retailerUrl, ProductCategory category = ProductCategory.Watch)
        : this(Guid.NewGuid(), slug, name, description, price, currency, colour, material, imageUrl, retailerUrl, DateTimeOffset.UtcNow, category)
    {
    }

    public Product(Guid id, string slug, string name, string description, decimal price, string currency, string colour, string material, string imageUrl, string retailerUrl, DateTimeOffset createdAt, ProductCategory category = ProductCategory.Watch)
    {
        Id = id;
        Slug = slug;
        SKU = slug.ToUpperInvariant();
        Name = name;
        Description = description;
        Price = price;
        Currency = currency;
        Colour = colour;
        Material = material;
        ImageUrl = imageUrl;
        RetailerUrl = retailerUrl;
        Category = category;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public Guid? TaxCategoryId { get; private set; }
    public decimal CostPrice { get; private set; }
    public bool TrackInventory { get; private set; } = true;
    public decimal ReorderLevel { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "PKR";
    public string Colour { get; private set; } = string.Empty;
    public string Material { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public string RetailerUrl { get; private set; } = string.Empty;
    public ProductCategory Category { get; private set; }
    public decimal? CaseDiameterMm { get; private set; }
    public bool IsActive { get; private set; }
    public bool TryOnReady { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public List<ProductOccasion> Occasions { get; private set; } = [];

    public void UpdateDetails(string slug, string name, string description, decimal price, string currency, string colour, string material, string retailerUrl, decimal? caseDiameterMm, ProductCategory category, Guid? categoryId = null)
    {
        Slug = slug;
        Name = name;
        Description = description;
        Price = price;
        Currency = currency;
        Colour = colour;
        Material = material;
        RetailerUrl = retailerUrl;
        CaseDiameterMm = caseDiameterMm;
        Category = category;
        CategoryId = categoryId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReplaceOccasions(IEnumerable<Occasion> occasions)
    {
        Occasions.Clear();
        Occasions.AddRange(occasions.Distinct().Select(occasion => new ProductOccasion(Id, occasion)));
    }

    public void SetPrimaryImage(string imageUrl, bool tryOnReady)
    {
        ImageUrl = imageUrl;
        TryOnReady = tryOnReady;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetTryOnReady(bool ready)
    {
        TryOnReady = ready;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public sealed class ProductOccasion
{
    private ProductOccasion() { }

    public ProductOccasion(Guid productId, Occasion occasion)
    {
        ProductId = productId;
        Occasion = occasion;
    }

    public Guid ProductId { get; private set; }
    public Occasion Occasion { get; private set; }
    public Product Product { get; private set; } = null!;
}
