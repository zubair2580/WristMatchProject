using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,CatalogManager")]
[ApiController]
[Route("api/v1/admin/categories")]
public sealed class AdminCategoriesController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(item => item.Name.Contains(search) || item.Slug.Contains(search));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Name).Skip((page - 1) * pageSize).Take(pageSize).Select(item => new CategoryResponse(item.Id, item.Name, item.Slug, item.Description, item.DisplayOrder, item.ParentCategoryId, item.IsActive, item.UpdatedAt, Convert.ToBase64String(item.RowVersion))).ToArrayAsync(cancellationToken);
        return Ok(new { items, page, pageSize, total, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryRequest request, CancellationToken cancellationToken)
    {
        var error = Validate(request); if (error is not null) return error;
        if (await db.Categories.AnyAsync(item => item.Slug == request.Slug, cancellationToken)) return Conflict(new { code = "SLUG_EXISTS", detail = "A category with this slug already exists." });
        var category = new Category(request.Name.Trim(), request.Slug.Trim(), request.Description.Trim(), request.DisplayOrder);
        category.Update(request.Name.Trim(), request.Slug.Trim(), request.Description.Trim(), request.DisplayOrder, request.ParentCategoryId, request.IsActive);
        db.Categories.Add(category); await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/categories/{category.Id}", new CategoryResponse(category.Id, category.Name, category.Slug, category.Description, category.DisplayOrder, category.ParentCategoryId, category.IsActive, category.UpdatedAt, Convert.ToBase64String(category.RowVersion)));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, CategoryRequest request, CancellationToken cancellationToken)
    {
        var error = Validate(request); if (error is not null) return error;
        var category = await db.Categories.SingleOrDefaultAsync(item => item.Id == id, cancellationToken); if (category is null) return NotFound();
        if (request.ParentCategoryId == id) return Problem(statusCode: 400, title: "Invalid parent", detail: "A category cannot be its own parent.");
        if (await db.Categories.AnyAsync(item => item.Slug == request.Slug && item.Id != id, cancellationToken)) return Conflict(new { code = "SLUG_EXISTS", detail = "A category with this slug already exists." });
        try { db.Entry(category).Property(item => item.RowVersion).OriginalValue = Convert.FromBase64String(request.RowVersion); } catch { return Conflict(new { code = "ROW_VERSION_INVALID", detail = "Refresh this category and try again." }); }
        category.Update(request.Name.Trim(), request.Slug.Trim(), request.Description.Trim(), request.DisplayOrder, request.ParentCategoryId, request.IsActive);
        try { await db.SaveChangesAsync(cancellationToken); } catch (DbUpdateConcurrencyException) { return Conflict(new { code = "CONCURRENCY_CONFLICT", detail = "Another administrator changed this category." }); }
        return Ok(new CategoryResponse(category.Id, category.Name, category.Slug, category.Description, category.DisplayOrder, category.ParentCategoryId, category.IsActive, category.UpdatedAt, Convert.ToBase64String(category.RowVersion)));
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.SingleOrDefaultAsync(item => item.Id == id, cancellationToken); if (category is null) return NotFound();
        category.Update(category.Name, category.Slug, category.Description, category.DisplayOrder, category.ParentCategoryId, false); await db.SaveChangesAsync(cancellationToken); return NoContent();
    }

    private ActionResult? Validate(CategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 120) return Problem(statusCode: 400, title: "Invalid name", detail: "Category name is required and must not exceed 120 characters.");
        if (string.IsNullOrWhiteSpace(request.Slug) || request.Slug.Length > 160 || request.Slug.Any(character => !(char.IsLower(character) || char.IsDigit(character) || character == '-'))) return Problem(statusCode: 400, title: "Invalid slug", detail: "Use lowercase letters, numbers, and hyphens only.");
        return null;
    }
}

public sealed record CategoryRequest(string Name, string Slug, string Description, int DisplayOrder, Guid? ParentCategoryId, bool IsActive, string RowVersion = "");
public sealed record CategoryResponse(Guid Id, string Name, string Slug, string Description, int DisplayOrder, Guid? ParentCategoryId, bool IsActive, DateTimeOffset UpdatedAt, string RowVersion);
