using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,CatalogManager")]
[ApiController]
[Route("api/v1/admin/products")]
public sealed class AdminProductsController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AdminProductResponse>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await db.Products.Include(product => product.Occasions).AsNoTracking().OrderByDescending(product => product.UpdatedAt).ToArrayAsync(cancellationToken);
        var categoryIds = products.Where(item => item.CategoryId.HasValue).Select(item => item.CategoryId!.Value).Distinct().ToArray();
        var categories = await db.Categories.AsNoTracking().Where(item => categoryIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        return Ok(products.Select(product => ToResponse(product, product.CategoryId is Guid id && categories.TryGetValue(id, out var name) ? name : null)).ToArray());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AdminProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var validation = await Validate(request, cancellationToken);
        if (validation is not null)
            return validation;
        if (await db.Products.AnyAsync(product => product.Slug == request.Slug, cancellationToken))
            return Conflict(new { code = "SLUG_EXISTS", detail = "A product with this slug already exists." });

        var product = new Product(request.Slug, request.Name, request.Description, request.Price, request.Currency, request.Colour, request.Material, request.ImageUrl, request.RetailerUrl, request.Category);
        product.UpdateDetails(request.Slug, request.Name, request.Description, request.Price, request.Currency, request.Colour, request.Material, request.RetailerUrl, request.CaseDiameterMm, request.Category, request.CategoryId);
        db.Products.Add(product);
        product.ReplaceOccasions(request.Occasions);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/products/{product.Id}", await ToResponseAsync(product, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AdminProductResponse>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var validation = await Validate(request, cancellationToken);
        if (validation is not null)
            return validation;
        var product = await db.Products.Include(item => item.Occasions).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (product is null)
            return NotFound();
        if (!string.Equals(product.Slug, request.Slug, StringComparison.OrdinalIgnoreCase) && await db.Products.AnyAsync(item => item.Slug == request.Slug && item.Id != id, cancellationToken))
            return Conflict(new { code = "SLUG_EXISTS", detail = "A product with this slug already exists." });
        if (!string.IsNullOrWhiteSpace(request.RowVersion))
        {
            try { db.Entry(product).Property(item => item.RowVersion).OriginalValue = Convert.FromBase64String(request.RowVersion); }
            catch (FormatException) { return Conflict(new { code = "ROW_VERSION_INVALID", detail = "The product was changed. Refresh and try again." }); }
        }
        product.UpdateDetails(request.Slug, request.Name, request.Description, request.Price, request.Currency, request.Colour, request.Material, request.RetailerUrl, request.CaseDiameterMm, request.Category, request.CategoryId);
        product.ReplaceOccasions(request.Occasions);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Conflict(new { code = "CONCURRENCY_CONFLICT", detail = "The product was changed by another administrator. Refresh and try again." }); }
        return Ok(await ToResponseAsync(product, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (product is null)
            return NotFound();
        product.Archive();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (product is null)
            return NotFound();
        product.Reactivate();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<AdminProductResponse> ToResponseAsync(Product product, CancellationToken cancellationToken)
    {
        string? categoryName = null;
        if (product.CategoryId is Guid categoryId)
            categoryName = await db.Categories.AsNoTracking().Where(item => item.Id == categoryId).Select(item => item.Name).SingleOrDefaultAsync(cancellationToken);
        return ToResponse(product, categoryName);
    }

    private static AdminProductResponse ToResponse(Product product, string? categoryName) => new(product.Id, product.Slug, product.Name, product.Description, product.Price, product.Currency, product.Colour, product.Material, product.ImageUrl, product.RetailerUrl, product.Category, product.CategoryId, categoryName, product.IsActive, product.TryOnReady, product.CaseDiameterMm, product.Occasions.Select(item => item.Occasion).ToArray(), Convert.ToBase64String(product.RowVersion));

    private async Task<ActionResult?> Validate(ProductRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Slug) || request.Slug.Length > 160 || request.Slug.Any(character => !(char.IsLetterOrDigit(character) || character == '-')))
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid slug", detail: "Use up to 160 lowercase letters, numbers, and hyphens.");
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 120 || request.Description.Length > 2000 || request.Price < 0 || request.Currency.Length != 3)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid product", detail: "Check the required product fields and limits.");
        var validProductImage = request.ImageUrl.StartsWith('/') || Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var image) && image.Scheme == Uri.UriSchemeHttps;
        if (!Uri.TryCreate(request.RetailerUrl, UriKind.Absolute, out var retailer) || retailer.Scheme != Uri.UriSchemeHttps || !validProductImage)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid URL", detail: "Retailer links must use HTTPS and product images must be HTTPS or an approved local path.");
        if (request.Occasions.Count == 0)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Occasion required", detail: "Select at least one occasion.");
        if (request.CategoryId.HasValue && !await db.Categories.AnyAsync(item => item.Id == request.CategoryId.Value && item.IsActive, cancellationToken))
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid category", detail: "Select an active catalogue category.");
        return null;
    }
}

public abstract record ProductRequest(string Slug, string Name, string Description, decimal Price, string Currency, string Colour, string Material, string ImageUrl, string RetailerUrl, ProductCategory Category, IReadOnlyCollection<Occasion> Occasions, decimal? CaseDiameterMm, Guid? CategoryId);
public sealed record CreateProductRequest(string Slug, string Name, string Description, decimal Price, string Currency, string Colour, string Material, string ImageUrl, string RetailerUrl, ProductCategory Category, IReadOnlyCollection<Occasion> Occasions, decimal? CaseDiameterMm = null, Guid? CategoryId = null) : ProductRequest(Slug, Name, Description, Price, Currency, Colour, Material, ImageUrl, RetailerUrl, Category, Occasions, CaseDiameterMm, CategoryId);
public sealed record UpdateProductRequest(string Slug, string Name, string Description, decimal Price, string Currency, string Colour, string Material, string ImageUrl, string RetailerUrl, ProductCategory Category, IReadOnlyCollection<Occasion> Occasions, decimal? CaseDiameterMm, string RowVersion, Guid? CategoryId = null) : ProductRequest(Slug, Name, Description, Price, Currency, Colour, Material, ImageUrl, RetailerUrl, Category, Occasions, CaseDiameterMm, CategoryId);
public sealed record AdminProductResponse(Guid Id, string Slug, string Name, string Description, decimal Price, string Currency, string Colour, string Material, string ImageUrl, string RetailerUrl, ProductCategory Category, Guid? CategoryId, string? CategoryName, bool IsActive, bool TryOnReady, decimal? CaseDiameterMm, IReadOnlyCollection<Occasion> Occasions, string RowVersion);
