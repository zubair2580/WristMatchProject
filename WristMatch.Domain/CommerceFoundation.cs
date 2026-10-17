namespace WristMatch.Domain;

public sealed class Category
{
    private Category() { }
    public Category(string name, string slug, string description, int displayOrder) { Id = Guid.NewGuid(); Name = name; Slug = slug; Description = description; DisplayOrder = displayOrder; IsActive = true; CreatedAt = UpdatedAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public void Update(string name, string slug, string description, int displayOrder, Guid? parentCategoryId, bool isActive) { Name = name; Slug = slug; Description = description; DisplayOrder = displayOrder; ParentCategoryId = parentCategoryId; IsActive = isActive; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class CustomerProfile
{
    private CustomerProfile() { }
    public CustomerProfile(string customerNumber, string firstName, string lastName, string email, string phone, bool isGuest) { Id = Guid.NewGuid(); CustomerNumber = customerNumber; FirstName = firstName; LastName = lastName; Email = email; Phone = phone; IsGuest = isGuest; IsActive = true; CreatedAt = UpdatedAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string CustomerNumber { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public bool IsGuest { get; private set; }
    public bool MarketingConsent { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public void Update(string firstName, string lastName, string email, string phone, bool marketingConsent, bool isGuest, bool isActive = true) { FirstName = firstName; LastName = lastName; Email = email; Phone = phone; MarketingConsent = marketingConsent; IsGuest = isGuest; IsActive = isActive; UpdatedAt = DateTimeOffset.UtcNow; }
    public void SetPasswordHash(string passwordHash) { PasswordHash = passwordHash; IsGuest = false; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Reactivate() { IsActive = true; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class CustomerAddress
{
    private CustomerAddress() { }
    public CustomerAddress(Guid customerId, string type, string recipientName, string phone, string addressLine1, string? addressLine2, string city, string? stateProvince, string? postalCode, string countryCode, bool isDefault)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Type = type;
        RecipientName = recipientName;
        Phone = phone;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        StateProvince = stateProvince;
        PostalCode = postalCode;
        CountryCode = string.IsNullOrWhiteSpace(countryCode) ? "PK" : countryCode.ToUpperInvariant();
        IsDefault = isDefault;
    }
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public CustomerProfile Customer { get; private set; } = null!;
    public string Type { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string AddressLine1 { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? StateProvince { get; private set; }
    public string? PostalCode { get; private set; }
    public string CountryCode { get; private set; } = "PK";
    public bool IsDefault { get; private set; }
}

public sealed class Warehouse
{
    private Warehouse() { }
    public Warehouse(string code, string name) { Id = Guid.NewGuid(); Code = code; Name = name; IsActive = true; CreatedAt = UpdatedAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public void Update(string code, string name, bool isActive) { Code = code; Name = name; IsActive = isActive; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class Supplier
{
    private Supplier() { }
    public Supplier(string supplierNumber, string name, string email, string phone, string currency) { Id = Guid.NewGuid(); SupplierNumber = supplierNumber; Name = name; Email = email; Phone = phone; Currency = currency; IsActive = true; CreatedAt = UpdatedAt = DateTimeOffset.UtcNow; }
    public Guid Id { get; private set; }
    public string SupplierNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? ContactName { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? TaxNumber { get; private set; }
    public int PaymentTermsDays { get; private set; }
    public string Currency { get; private set; } = "PKR";
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public void Update(string name, string email, string phone, string currency, int paymentTermsDays, bool isActive) { Name = name; Email = email; Phone = phone; Currency = currency; PaymentTermsDays = paymentTermsDays; IsActive = isActive; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class TaxCategory
{
    private TaxCategory() { }
    public TaxCategory(string name, string description) { Id = Guid.NewGuid(); Name = name; Description = description; IsActive = true; }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
}

public sealed class TaxRate
{
    private TaxRate() { }
    public Guid Id { get; private set; }
    public Guid TaxCategoryId { get; private set; }
    public TaxCategory TaxCategory { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public decimal Rate { get; private set; }
    public string CountryCode { get; private set; } = "PK";
    public string? StateProvince { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }
}
