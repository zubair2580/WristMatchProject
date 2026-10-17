using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,OperationsManager,PurchasingManager")]
[ApiController]
[Route("api/v1/admin/warehouses")]
public sealed class AdminWarehousesController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100); var query = db.Warehouses.AsNoTracking(); if (!string.IsNullOrWhiteSpace(search)) query = query.Where(item => item.Code.Contains(search) || item.Name.Contains(search)); var total = await query.CountAsync(cancellationToken); var items = await query.OrderBy(item => item.Code).Skip((page - 1) * pageSize).Take(pageSize).Select(item => new WarehouseResponse(item.Id, item.Code, item.Name, item.IsActive, item.CreatedAt, Convert.ToBase64String(item.RowVersion))).ToArrayAsync(cancellationToken); return Ok(new { items, page, pageSize, total, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WarehouseRequest request, CancellationToken cancellationToken)
    {
        var validation = Validate(request); if (validation is not null) return validation; if (await db.Warehouses.AnyAsync(item => item.Code == request.Code, cancellationToken)) return Conflict(new { code = "CODE_EXISTS", detail = "A warehouse with this code already exists." }); var warehouse = new Warehouse(request.Code.Trim().ToUpperInvariant(), request.Name.Trim()); warehouse.Update(request.Code.Trim().ToUpperInvariant(), request.Name.Trim(), request.IsActive); db.Warehouses.Add(warehouse); await db.SaveChangesAsync(cancellationToken); return Created($"/api/v1/admin/warehouses/{warehouse.Id}", ToResponse(warehouse));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, WarehouseRequest request, CancellationToken cancellationToken)
    {
        var validation = Validate(request); if (validation is not null) return validation; var warehouse = await db.Warehouses.SingleOrDefaultAsync(item => item.Id == id, cancellationToken); if (warehouse is null) return NotFound(); if (await db.Warehouses.AnyAsync(item => item.Code == request.Code && item.Id != id, cancellationToken)) return Conflict(new { code = "CODE_EXISTS", detail = "A warehouse with this code already exists." }); try { db.Entry(warehouse).Property(item => item.RowVersion).OriginalValue = Convert.FromBase64String(request.RowVersion); } catch { return Conflict(new { code = "ROW_VERSION_INVALID", detail = "Refresh this warehouse and try again." }); } warehouse.Update(request.Code.Trim().ToUpperInvariant(), request.Name.Trim(), request.IsActive); try { await db.SaveChangesAsync(cancellationToken); } catch (DbUpdateConcurrencyException) { return Conflict(new { code = "CONCURRENCY_CONFLICT", detail = "Another administrator changed this warehouse." }); } return Ok(ToResponse(warehouse));
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken) { var warehouse = await db.Warehouses.SingleOrDefaultAsync(item => item.Id == id, cancellationToken); if (warehouse is null) return NotFound(); warehouse.Update(warehouse.Code, warehouse.Name, false); await db.SaveChangesAsync(cancellationToken); return NoContent(); }
    private static WarehouseResponse ToResponse(Warehouse item) => new(item.Id, item.Code, item.Name, item.IsActive, item.CreatedAt, Convert.ToBase64String(item.RowVersion));
    private static ActionResult? Validate(WarehouseRequest request) => string.IsNullOrWhiteSpace(request.Code) || request.Code.Length > 30 || string.IsNullOrWhiteSpace(request.Name) ? new BadRequestObjectResult(new { code = "INVALID_WAREHOUSE", detail = "Warehouse code and name are required." }) : null;
}

public sealed record WarehouseRequest(string Code, string Name, bool IsActive, string RowVersion = "");
public sealed record WarehouseResponse(Guid Id, string Code, string Name, bool IsActive, DateTimeOffset CreatedAt, string RowVersion);
