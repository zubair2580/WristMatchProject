using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WristMatch.Application;
using WristMatch.Domain;

namespace WristMatch.Infrastructure;

public sealed class WristMatchDbContext(DbContextOptions<WristMatchDbContext> options) : IdentityDbContext<AdminUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options), IProductReadStore
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductOccasion> ProductOccasions => Set<ProductOccasion>();
    public DbSet<GuestSession> GuestSessions => Set<GuestSession>();
    public DbSet<PhotoAsset> PhotoAssets => Set<PhotoAsset>();
    public DbSet<ProductAsset> ProductAssets => Set<ProductAsset>();
    public DbSet<ProductAssetLicence> ProductAssetLicences => Set<ProductAssetLicence>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<TaxCategory> TaxCategories => Set<TaxCategory>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<AdminMenuPermission> AdminMenuPermissions => Set<AdminMenuPermission>();
    public DbSet<TryOnJob> TryOnJobs => Set<TryOnJob>();
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceLine> SalesInvoiceLines => Set<SalesInvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();

    public Task<IReadOnlyCollection<Product>> QueryAsync(CancellationToken cancellationToken) => Products.Include(product => product.Occasions).AsNoTracking().ToArrayAsync(cancellationToken).ContinueWith(task => (IReadOnlyCollection<Product>)task.Result, cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.CreatedAt).IsRequired();
            entity.Property(user => user.UpdatedAt).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.HasIndex(product => new { product.IsActive, product.Category, product.Price });
            entity.HasIndex(product => product.Slug).IsUnique();
            entity.HasIndex(product => product.SKU).IsUnique();
            entity.Property(product => product.SKU).HasMaxLength(80).IsRequired();
            entity.Property(product => product.Price).HasPrecision(18, 2);
            entity.Property(product => product.CostPrice).HasPrecision(18, 2);
            entity.Property(product => product.ReorderLevel).HasPrecision(18, 4);
            entity.Property(product => product.CaseDiameterMm).HasPrecision(5, 2);
            entity.Property(product => product.Currency).HasMaxLength(3).IsRequired();
            entity.Property(product => product.Name).HasMaxLength(120).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(2000).IsRequired();
            entity.Property(product => product.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(product => product.RetailerUrl).HasMaxLength(500).IsRequired();
            entity.Property(product => product.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<ProductOccasion>(entity =>
        {
            entity.HasKey(item => new { item.ProductId, item.Occasion });
            entity.HasOne(item => item.Product).WithMany(product => product.Occasions).HasForeignKey(item => item.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(category => category.Id);
            entity.HasIndex(category => category.Slug).IsUnique();
            entity.Property(category => category.Name).HasMaxLength(120).IsRequired();
            entity.Property(category => category.Slug).HasMaxLength(160).IsRequired();
            entity.Property(category => category.Description).HasMaxLength(1000);
            entity.Property(category => category.RowVersion).IsRowVersion();
            entity.HasOne(category => category.ParentCategory).WithMany().HasForeignKey(category => category.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerProfile>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.HasIndex(customer => customer.CustomerNumber).IsUnique();
            entity.HasIndex(customer => customer.Email);
            entity.Property(customer => customer.CustomerNumber).HasMaxLength(40).IsRequired();
            entity.Property(customer => customer.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(customer => customer.LastName).HasMaxLength(100).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(256).IsRequired();
            entity.Property(customer => customer.Phone).HasMaxLength(40).IsRequired();
            entity.Property(customer => customer.PasswordHash).HasMaxLength(500);
            entity.Property(customer => customer.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.HasKey(address => address.Id);
            entity.Property(address => address.Type).HasMaxLength(30).IsRequired();
            entity.Property(address => address.RecipientName).HasMaxLength(200).IsRequired();
            entity.Property(address => address.AddressLine1).HasMaxLength(300).IsRequired();
            entity.Property(address => address.CountryCode).HasMaxLength(2).IsRequired();
            entity.HasOne(address => address.Customer).WithMany().HasForeignKey(address => address.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Warehouse>(entity => { entity.HasKey(item => item.Id); entity.HasIndex(item => item.Code).IsUnique(); entity.Property(item => item.Code).HasMaxLength(30).IsRequired(); entity.Property(item => item.Name).HasMaxLength(160).IsRequired(); entity.Property(item => item.RowVersion).IsRowVersion(); });
        modelBuilder.Entity<Supplier>(entity => { entity.HasKey(item => item.Id); entity.HasIndex(item => item.SupplierNumber).IsUnique(); entity.Property(item => item.SupplierNumber).HasMaxLength(40).IsRequired(); entity.Property(item => item.Name).HasMaxLength(200).IsRequired(); entity.Property(item => item.Currency).HasMaxLength(3).IsRequired(); entity.Property(item => item.RowVersion).IsRowVersion(); });
        modelBuilder.Entity<TaxCategory>(entity => { entity.HasKey(item => item.Id); entity.HasIndex(item => item.Name).IsUnique(); entity.Property(item => item.Name).HasMaxLength(120).IsRequired(); });
        modelBuilder.Entity<TaxRate>(entity => { entity.HasKey(item => item.Id); entity.Property(item => item.Rate).HasPrecision(9, 6); entity.Property(item => item.Name).HasMaxLength(120).IsRequired(); entity.Property(item => item.CountryCode).HasMaxLength(2).IsRequired(); entity.HasOne(item => item.TaxCategory).WithMany().HasForeignKey(item => item.TaxCategoryId).OnDelete(DeleteBehavior.Restrict); });
        modelBuilder.Entity<AdminMenuPermission>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.RoleName, item.MenuKey }).IsUnique();
            entity.Property(item => item.RoleName).HasMaxLength(80).IsRequired();
            entity.Property(item => item.MenuKey).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<ProductAsset>(entity =>
        {
            entity.HasKey(asset => asset.Id);
            entity.HasIndex(asset => new { asset.ProductId, asset.IsApproved, asset.IsPrimary });
            entity.Property(asset => asset.StorageKey).HasMaxLength(300).IsRequired();
            entity.Property(asset => asset.MimeType).HasMaxLength(100).IsRequired();
            entity.HasOne(asset => asset.Product).WithMany().HasForeignKey(asset => asset.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductAssetLicence>(entity =>
        {
            entity.HasKey(licence => licence.Id);
            entity.HasIndex(licence => licence.ProductAssetId).IsUnique();
            entity.Property(licence => licence.Source).HasMaxLength(300).IsRequired();
            entity.Property(licence => licence.LicenceNote).HasMaxLength(1000).IsRequired();
            entity.Property(licence => licence.Attribution).HasMaxLength(500);
            entity.HasOne(licence => licence.ProductAsset).WithOne(asset => asset.Licence).HasForeignKey<ProductAssetLicence>(licence => licence.ProductAssetId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GuestSession>(entity =>
        {
            entity.HasKey(session => session.Id);
            entity.HasIndex(session => session.TokenHash).IsUnique();
            entity.Property(session => session.TokenHash).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<PhotoAsset>(entity =>
        {
            entity.HasKey(photo => photo.Id);
            entity.HasIndex(photo => new { photo.SessionId, photo.ExpiresAt });
            entity.Property(photo => photo.StorageKey).HasMaxLength(300).IsRequired();
            entity.Property(photo => photo.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(photo => photo.ConsentVersion).HasMaxLength(50).IsRequired();
            entity.HasOne(photo => photo.Session).WithMany(session => session.Photos).HasForeignKey(photo => photo.SessionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TryOnJob>(entity =>
        {
            entity.ToTable("TryOnJobs");
            entity.HasKey(job => job.Id);
            entity.HasIndex(job => new { job.SessionId, job.IdempotencyKey }).IsUnique();
            entity.HasIndex(job => new { job.State, job.CreatedAt });
            entity.Property(job => job.IdempotencyKey).HasMaxLength(120).IsRequired();
            entity.Property(job => job.RequestHash).HasMaxLength(64).IsRequired();
            entity.Property(job => job.ProviderMode).HasMaxLength(40).IsRequired();
            entity.Property(job => job.ProviderTaskId).HasMaxLength(120);
            entity.Property(job => job.ResultStorageKey).HasMaxLength(300);
            entity.Property(job => job.SafeMessage).HasMaxLength(500);
            entity.Property(job => job.State).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<InventoryBalance>(entity =>
        {
            entity.ToTable("InventoryBalances");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.WarehouseId, item.ProductId }).IsUnique();
            entity.Property(item => item.OnHand).HasPrecision(18, 4);
            entity.Property(item => item.Reserved).HasPrecision(18, 4);
            entity.Property(item => item.AverageCost).HasPrecision(18, 6);
            entity.Property(item => item.RowVersion).IsRowVersion();
            entity.Ignore(item => item.Available);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("StockMovements");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.MovementType).HasMaxLength(40).IsRequired();
            entity.Property(item => item.Quantity).HasPrecision(18, 4);
            entity.Property(item => item.UnitCost).HasPrecision(18, 6);
            entity.Property(item => item.TotalCost).HasPrecision(18, 2);
            entity.Property(item => item.ReferenceNumber).HasMaxLength(60).IsRequired();
            entity.Property(item => item.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.ToTable("SalesOrders");
            entity.HasKey(order => order.Id);
            entity.HasIndex(order => order.OrderNumber).IsUnique();
            entity.Property(order => order.OrderNumber).HasMaxLength(40).IsRequired();
            entity.Property(order => order.Currency).HasMaxLength(3).IsRequired();
            entity.Property(order => order.Notes).HasMaxLength(500);
            entity.Property(order => order.Subtotal).HasPrecision(18, 2);
            entity.Property(order => order.TaxTotal).HasPrecision(18, 2);
            entity.Property(order => order.GrandTotal).HasPrecision(18, 2);
            entity.Property(order => order.TaxRate).HasPrecision(9, 6);
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(order => order.PaymentStatus).HasConversion<string>().HasMaxLength(30);
            entity.Property(order => order.FulfilmentStatus).HasConversion<string>().HasMaxLength(30);
            entity.HasMany(order => order.Lines).WithOne(line => line.SalesOrder).HasForeignKey(line => line.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.Ignore(order => order.HasStockReservation);
        });

        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.ToTable("SalesOrderLines");
            entity.HasKey(line => line.Id);
            entity.Property(line => line.Quantity).HasPrecision(18, 4);
            entity.Property(line => line.UnitPrice).HasPrecision(18, 2);
            entity.Property(line => line.TaxAmount).HasPrecision(18, 2);
            entity.Property(line => line.LineTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.ToTable("PurchaseOrders");
            entity.HasKey(order => order.Id);
            entity.HasIndex(order => order.PurchaseOrderNumber).IsUnique();
            entity.Property(order => order.PurchaseOrderNumber).HasMaxLength(40).IsRequired();
            entity.Property(order => order.Currency).HasMaxLength(3).IsRequired();
            entity.Property(order => order.Notes).HasMaxLength(500);
            entity.Property(order => order.Subtotal).HasPrecision(18, 2);
            entity.Property(order => order.TaxTotal).HasPrecision(18, 2);
            entity.Property(order => order.GrandTotal).HasPrecision(18, 2);
            entity.Property(order => order.TaxRate).HasPrecision(9, 6);
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasMany(order => order.Lines).WithOne(line => line.PurchaseOrder).HasForeignKey(line => line.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.ToTable("PurchaseOrderLines");
            entity.HasKey(line => line.Id);
            entity.Property(line => line.Quantity).HasPrecision(18, 4);
            entity.Property(line => line.ReceivedQuantity).HasPrecision(18, 4);
            entity.Property(line => line.UnitCost).HasPrecision(18, 6);
            entity.Property(line => line.LineTotal).HasPrecision(18, 2);
            entity.Ignore(line => line.OutstandingQuantity);
        });

        modelBuilder.Entity<SalesInvoice>(entity =>
        {
            entity.ToTable("SalesInvoices");
            entity.HasKey(invoice => invoice.Id);
            entity.Property(invoice => invoice.Id).ValueGeneratedNever();
            entity.HasIndex(invoice => invoice.InvoiceNumber).IsUnique();
            entity.Property(invoice => invoice.InvoiceNumber).HasMaxLength(40).IsRequired();
            entity.Property(invoice => invoice.Currency).HasColumnType("char(3)").IsFixedLength().IsRequired();
            entity.Property(invoice => invoice.Notes).HasMaxLength(500);
            entity.Property(invoice => invoice.Subtotal).HasPrecision(18, 2);
            entity.Property(invoice => invoice.TaxTotal).HasPrecision(18, 2);
            entity.Property(invoice => invoice.GrandTotal).HasPrecision(18, 2);
            entity.Property(invoice => invoice.OutstandingAmount).HasPrecision(18, 2);
            entity.Property(invoice => invoice.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasMany(invoice => invoice.Lines).WithOne(line => line.SalesInvoice).HasForeignKey(line => line.SalesInvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(invoice => invoice.Payments).WithOne(payment => payment.SalesInvoice).HasForeignKey(payment => payment.SalesInvoiceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalesInvoiceLine>(entity =>
        {
            entity.ToTable("SalesInvoiceLines");
            entity.HasKey(line => line.Id);
            entity.Property(line => line.Id).ValueGeneratedNever();
            entity.Property(line => line.Quantity).HasPrecision(18, 4);
            entity.Property(line => line.UnitPrice).HasPrecision(18, 2);
            entity.Property(line => line.TaxAmount).HasPrecision(18, 2);
            entity.Property(line => line.LineTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(payment => payment.Id);
            entity.Property(payment => payment.Id).ValueGeneratedNever();
            entity.HasIndex(payment => payment.PaymentNumber).IsUnique();
            entity.Property(payment => payment.PaymentNumber).HasMaxLength(40).IsRequired();
            entity.Property(payment => payment.Method).HasMaxLength(30).IsRequired();
            entity.Property(payment => payment.Currency).HasColumnType("char(3)").IsFixedLength().IsRequired();
            entity.Property(payment => payment.Amount).HasPrecision(18, 2);
            entity.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(30);
        });

        Seed(modelBuilder);
        if (Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            SeedCommerceFoundation(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTimeOffset(2026, 10, 11, 0, 0, 0, TimeSpan.Zero);
        var products = new[]
        {
            new Product(Guid.Parse("c0b6b7f7-945b-4bd1-bf73-8c77d7b25a01"), "meridian-steel", "Meridian Steel", "A clean everyday watch with a brushed steel bracelet and balanced proportions.", 18500, "PKR", "Silver", "Stainless Steel", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85", "https://example.com/meridian-steel", createdAt),
            new Product(Guid.Parse("d7b7f7a2-0a53-4f13-a7a0-0f8a9d9b0f02"), "noir-atelier", "Noir Atelier", "A refined black dial with a leather strap for considered evening dressing.", 24900, "PKR", "Black", "Leather", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85", "https://example.com/noir-atelier", createdAt),
            new Product(Guid.Parse("e9e5c453-3188-470f-b8b5-43f1e1f0e303"), "champagne-classic", "Champagne Classic", "Warm champagne tones and a slim case designed for formal occasions.", 31900, "PKR", "Gold", "Stainless Steel", "https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=900&q=85", "https://example.com/champagne-classic", createdAt),
            new Product(Guid.Parse("f4a1e3b8-8c1d-4c7e-9a0a-2f6d1b7c4404"), "field-olive", "Field Olive", "A practical field-inspired watch with a durable fabric strap.", 14200, "PKR", "Green", "Nylon", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=900&q=85", "https://example.com/field-olive", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000005"), "harbour-blue", "Harbour Blue", "A deep blue dial with a polished steel profile for versatile daily wear.", 22500, "PKR", "Blue", "Stainless Steel", "https://images.unsplash.com/photo-1526045431048-f857369baa09?auto=format&fit=crop&w=900&q=85", "https://example.com/harbour-blue", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000006"), "rose-hour", "Rose Hour", "A warm rose-tone watch with a minimal face and refined proportions.", 27800, "PKR", "Rose Gold", "Stainless Steel", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85", "https://example.com/rose-hour", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000007"), "slate-office", "Slate Office", "A charcoal dial and clean markers created for understated office dressing.", 19800, "PKR", "Charcoal", "Stainless Steel", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85", "https://example.com/slate-office", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000008"), "heritage-brown", "Heritage Brown", "A classic brown leather strap paired with a warm, traditional dial.", 21400, "PKR", "Brown", "Leather", "https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=900&q=85", "https://example.com/heritage-brown", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000009"), "lunar-silver", "Lunar Silver", "A bright silver bracelet and quiet dial with contemporary detailing.", 26400, "PKR", "Silver", "Stainless Steel", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=900&q=85", "https://example.com/lunar-silver", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000010"), "midnight-mesh", "Midnight Mesh", "A monochrome watch with a flexible mesh bracelet for evening wear.", 28900, "PKR", "Black", "Stainless Steel", "https://images.unsplash.com/photo-1526045431048-f857369baa09?auto=format&fit=crop&w=900&q=85", "https://example.com/midnight-mesh", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000011"), "sandstone-minimal", "Sandstone Minimal", "A calm neutral dial and slim case designed for effortless styling.", 17600, "PKR", "Beige", "Leather", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85", "https://example.com/sandstone-minimal", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000012"), "copper-atelier", "Copper Atelier", "A copper-tone statement watch with balanced contemporary geometry.", 33600, "PKR", "Copper", "Stainless Steel", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85", "https://example.com/copper-atelier", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000013"), "weekend-canvas", "Weekend Canvas", "A relaxed watch with a durable canvas strap for casual weekends.", 12900, "PKR", "Navy", "Canvas", "https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=900&q=85", "https://example.com/weekend-canvas", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000014"), "ivory-ceremony", "Ivory Ceremony", "An ivory dial with polished accents for formal celebrations.", 35200, "PKR", "Ivory", "Stainless Steel", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=900&q=85", "https://example.com/ivory-ceremony", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000015"), "graphite-motion", "Graphite Motion", "A graphite sports-inspired profile with practical everyday character.", 23800, "PKR", "Graphite", "Silicone", "https://images.unsplash.com/photo-1526045431048-f857369baa09?auto=format&fit=crop&w=900&q=85", "https://example.com/graphite-motion", createdAt),
            new Product(Guid.Parse("10000000-0000-4000-8000-000000000016"), "classic-gift", "Classic Gift", "A timeless polished watch presented as a versatile gift choice.", 30500, "PKR", "Gold", "Stainless Steel", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=900&q=85", "https://example.com/classic-gift", createdAt)
        };
        foreach (var product in products)
        {
            modelBuilder.Entity<Product>().HasData(product);
            var occasions = product.Name switch
            {
                "Meridian Steel" => new[] { Occasion.Office, Occasion.Casual, Occasion.Gift },
                "Noir Atelier" => new[] { Occasion.Office, Occasion.Wedding, Occasion.Gift },
                "Champagne Classic" => new[] { Occasion.Wedding, Occasion.Gift },
                _ => new[] { Occasion.Casual, Occasion.Office }
            };
            foreach (var occasion in occasions)
                modelBuilder.Entity<ProductOccasion>().HasData(new { ProductId = product.Id, Occasion = occasion });
        }
    }

    private static void SeedCommerceFoundation(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTimeOffset(2026, 10, 11, 0, 0, 0, TimeSpan.Zero);
        var categories = new[]
        {
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000001"), Name = "Everyday Watches", Slug = "everyday-watches", Description = "Practical watches for daily wear.", DisplayOrder = 1 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000002"), Name = "Dress Watches", Slug = "dress-watches", Description = "Refined watches for formal occasions.", DisplayOrder = 2 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000003"), Name = "Field Watches", Slug = "field-watches", Description = "Durable field-inspired watches.", DisplayOrder = 3 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000004"), Name = "Steel Collection", Slug = "steel-collection", Description = "Stainless steel bracelet watches.", DisplayOrder = 4 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000005"), Name = "Leather Collection", Slug = "leather-collection", Description = "Leather strap watches.", DisplayOrder = 5 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000006"), Name = "Minimal Watches", Slug = "minimal-watches", Description = "Clean, understated dial designs.", DisplayOrder = 6 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000007"), Name = "Statement Watches", Slug = "statement-watches", Description = "Distinctive watches with a strong presence.", DisplayOrder = 7 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000008"), Name = "Gift Watches", Slug = "gift-watches", Description = "Considered watches for gifting.", DisplayOrder = 8 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000009"), Name = "Office Edit", Slug = "office-edit", Description = "Professional watches for the working week.", DisplayOrder = 9 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000010"), Name = "Wedding Edit", Slug = "wedding-edit", Description = "Formal watches for celebrations.", DisplayOrder = 10 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000011"), Name = "Weekend Edit", Slug = "weekend-edit", Description = "Relaxed watches for weekends.", DisplayOrder = 11 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000012"), Name = "Black Dial", Slug = "black-dial", Description = "Black dial watches.", DisplayOrder = 12 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000013"), Name = "Blue Dial", Slug = "blue-dial", Description = "Blue dial watches.", DisplayOrder = 13 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000014"), Name = "Gold Tone", Slug = "gold-tone", Description = "Warm gold and champagne finishes.", DisplayOrder = 14 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000015"), Name = "Neutral Tones", Slug = "neutral-tones", Description = "Ivory, beige, and sandstone tones.", DisplayOrder = 15 },
            new { Id = Guid.Parse("20000000-0000-4000-8000-000000000016"), Name = "Sample Archive", Slug = "sample-archive", Description = "Inactive testing category.", DisplayOrder = 16 }
        };
        foreach (var category in categories)
        {
            modelBuilder.Entity<Category>().HasData(new
            {
                category.Id,
                ParentCategoryId = (Guid?)null,
                category.Name,
                category.Slug,
                category.Description,
                category.DisplayOrder,
                IsActive = category.Slug != "sample-archive",
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            });
        }

        var customers = Enumerable.Range(1, 16).Select(index => new
        {
            Id = Guid.Parse($"21000000-0000-4000-8000-{index:000000000000}"),
            CustomerNumber = $"CUS-DEMO-{index:0000}",
            FirstName = $"Demo{index}",
            LastName = "Customer",
            Email = $"demo.customer{index}@wristmatch.local",
            Phone = $"+92 300 555 {index:0000}",
            IsGuest = index % 2 == 0,
            MarketingConsent = index % 3 == 0,
            IsActive = index != 16,
            CreatedAt = createdAt.AddDays(-index),
            UpdatedAt = createdAt.AddDays(-index)
        }).ToArray();
        foreach (var customer in customers)
            modelBuilder.Entity<CustomerProfile>().HasData(customer);

        var suppliers = Enumerable.Range(1, 16).Select(index => new
        {
            Id = Guid.Parse($"22000000-0000-4000-8000-{index:000000000000}"),
            SupplierNumber = $"SUP-DEMO-{index:0000}",
            Name = $"Demo Supply Partner {index}",
            ContactName = $"Contact {index}",
            Email = $"supplier{index}@demo.wristmatch.local",
            Phone = $"+92 301 666 {index:0000}",
            TaxNumber = $"DEMO-TAX-{index:0000}",
            PaymentTermsDays = 15 + (index % 4) * 15,
            Currency = "PKR",
            IsActive = index != 16,
            CreatedAt = createdAt.AddDays(-index),
            UpdatedAt = createdAt.AddDays(-index)
        }).ToArray();
        foreach (var supplier in suppliers)
            modelBuilder.Entity<Supplier>().HasData(supplier);

        var warehouses = Enumerable.Range(1, 16).Select(index => new
        {
            Id = Guid.Parse($"23000000-0000-4000-8000-{index:000000000000}"),
            Code = $"WH-{index:0000}",
            Name = $"Demo Warehouse {index}",
            IsActive = index != 16,
            CreatedAt = createdAt.AddDays(-index),
            UpdatedAt = createdAt.AddDays(-index)
        }).ToArray();
        foreach (var warehouse in warehouses)
            modelBuilder.Entity<Warehouse>().HasData(warehouse);

        var taxCategories = new[]
        {
            new { Id = Guid.Parse("24000000-0000-4000-8000-000000000001"), Name = "Standard Demo Tax", Description = "Demo configurable standard rate.", IsActive = true },
            new { Id = Guid.Parse("24000000-0000-4000-8000-000000000002"), Name = "Reduced Demo Tax", Description = "Demo configurable reduced rate.", IsActive = true },
            new { Id = Guid.Parse("24000000-0000-4000-8000-000000000003"), Name = "Zero Demo Tax", Description = "Demo zero rate.", IsActive = true }
        };
        foreach (var taxCategory in taxCategories)
            modelBuilder.Entity<TaxCategory>().HasData(taxCategory);
    }
}
