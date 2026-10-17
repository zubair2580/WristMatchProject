using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,OperationsManager,PurchasingManager")]
[ApiController]
[Route("api/v1/admin/stock")]
public sealed class AdminStockController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBalances([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = from balance in db.InventoryBalances.AsNoTracking()
                    join product in db.Products.AsNoTracking() on balance.ProductId equals product.Id
                    join warehouse in db.Warehouses.AsNoTracking() on balance.WarehouseId equals warehouse.Id
                    select new
                    {
                        balance.Id,
                        SKU = product.SKU,
                        Product = product.Name,
                        Warehouse = warehouse.Code,
                        OnHand = balance.OnHand,
                        Reserved = balance.Reserved,
                        Available = balance.OnHand - balance.Reserved,
                        AverageCost = balance.AverageCost,
                        balance.RowVersion
                    };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item => item.SKU.ToLower().Contains(term) || item.Product.ToLower().Contains(term) || item.Warehouse.ToLower().Contains(term));
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(item => item.Warehouse).ThenBy(item => item.SKU).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);
        return Ok(new
        {
            items = items.Select(item => new Dictionary<string, object?>
            {
                ["SKU"] = item.SKU,
                ["Product"] = item.Product,
                ["Warehouse"] = item.Warehouse,
                ["On hand"] = item.OnHand,
                ["Reserved"] = item.Reserved,
                ["Available"] = item.Available,
                ["Average cost"] = item.AverageCost
            }),
            page,
            pageSize,
            total,
            totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize))
        });
    }

    [HttpPost("adjustments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        if (request.QuantityDelta == 0)
            return BadRequest(new { code = "INVALID_QUANTITY", detail = "Quantity delta cannot be zero." });
        if (request.QuantityDelta > 0 && (!request.UnitCost.HasValue || request.UnitCost < 0))
            return BadRequest(new { code = "UNIT_COST_REQUIRED", detail = "Positive adjustments require a unit cost." });

        var warehouse = await db.Warehouses.SingleOrDefaultAsync(item => item.Id == request.WarehouseId && item.IsActive, cancellationToken);
        var product = await db.Products.SingleOrDefaultAsync(item => item.Id == request.ProductId && item.IsActive, cancellationToken);
        if (warehouse is null || product is null)
            return BadRequest(new { code = "INVALID_REFERENCES", detail = "Choose an active warehouse and product." });

        var balance = await db.InventoryBalances.SingleOrDefaultAsync(item => item.WarehouseId == request.WarehouseId && item.ProductId == request.ProductId, cancellationToken);
        if (balance is null)
        {
            balance = new InventoryBalance(request.WarehouseId, request.ProductId);
            db.InventoryBalances.Add(balance);
        }

        try
        {
            balance.ApplyAdjustment(request.QuantityDelta, request.UnitCost, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { code = "STOCK_RULE", detail = exception.Message });
        }

        var movement = new StockMovement(request.WarehouseId, request.ProductId, request.QuantityDelta > 0 ? "AdjustmentIn" : "AdjustmentOut", request.QuantityDelta, request.UnitCost ?? balance.AverageCost, request.Reference?.Trim() is { Length: > 0 } reference ? reference : $"ADJ-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}", request.Notes);
        db.StockMovements.Add(movement);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { balanceId = balance.Id, balance.OnHand, balance.Reserved, Available = balance.Available, balance.AverageCost, movementId = movement.Id });
    }
}

public sealed record StockAdjustmentRequest(Guid WarehouseId, Guid ProductId, decimal QuantityDelta, decimal? UnitCost, string? Reference, string? Notes);
