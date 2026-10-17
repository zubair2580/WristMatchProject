using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,PurchasingManager,OperationsManager")]
[ApiController]
[Route("api/v1/admin/purchases")]
public sealed class AdminPurchasesController(WristMatchDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = from order in db.PurchaseOrders.AsNoTracking()
                    join supplier in db.Suppliers.AsNoTracking() on order.SupplierId equals supplier.Id
                    join warehouse in db.Warehouses.AsNoTracking() on order.WarehouseId equals warehouse.Id
                    select new
                    {
                        order.Id,
                        PurchaseOrder = order.PurchaseOrderNumber,
                        Supplier = supplier.Name,
                        Warehouse = warehouse.Code,
                        Ordered = order.OrderedAt,
                        Expected = order.ExpectedAt,
                        Total = order.GrandTotal,
                        Status = order.Status.ToString(),
                        order.Currency
                    };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item => item.PurchaseOrder.ToLower().Contains(term) || item.Supplier.ToLower().Contains(term) || item.Status.ToLower().Contains(term) || item.Warehouse.ToLower().Contains(term));
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(item => item.Ordered).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);
        return Ok(new { items, page, pageSize, total, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)) });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var detail = await BuildDetail(id, cancellationToken);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
            return BadRequest(new { code = "LINES_REQUIRED", detail = "Add at least one purchase line." });
        var supplier = await db.Suppliers.SingleOrDefaultAsync(item => item.Id == request.SupplierId && item.IsActive, cancellationToken);
        var warehouse = await db.Warehouses.SingleOrDefaultAsync(item => item.Id == request.WarehouseId && item.IsActive, cancellationToken);
        if (supplier is null || warehouse is null)
            return BadRequest(new { code = "INVALID_REFERENCES", detail = "Choose an active supplier and warehouse." });

        var productIds = request.Lines.Select(line => line.ProductId).Distinct().ToArray();
        var products = await db.Products.Where(item => productIds.Contains(item.Id) && item.IsActive).ToDictionaryAsync(item => item.Id, cancellationToken);
        if (products.Count != productIds.Length)
            return BadRequest(new { code = "INVALID_PRODUCT", detail = "One or more products are inactive or missing." });

        var taxRate = configuration.GetValue("Commerce:DefaultTaxRate", 0.10m);
        var number = $"PO-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var order = new PurchaseOrder(number, request.SupplierId, request.WarehouseId, request.Currency ?? supplier.Currency, taxRate, request.ExpectedAt);
        order.SetNotes(request.Notes);
        order.ReplaceLines(request.Lines.Select(line =>
        {
            var product = products[line.ProductId];
            var unitCost = line.UnitCost ?? (product.CostPrice > 0 ? product.CostPrice : product.Price * 0.6m);
            return (line.ProductId, line.Quantity, unitCost);
        }));
        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/purchases/{order.Id}", await BuildDetail(order.Id, cancellationToken));
    }

    [HttpPost("{id:guid}/transitions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(Guid id, PurchaseTransitionRequest request, CancellationToken cancellationToken)
    {
        var order = await db.PurchaseOrders.Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (order is null) return NotFound();

        try
        {
            switch (request.Action.Trim().ToLowerInvariant())
            {
                case "approve":
                    order.Approve();
                    break;
                case "receive":
                    await ReceiveGoods(order, request.Lines ?? [], cancellationToken);
                    break;
                case "cancel":
                    order.Cancel();
                    break;
                default:
                    return BadRequest(new { code = "UNKNOWN_ACTION", detail = "Use approve, receive, or cancel." });
            }
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { code = "PURCHASE_RULE", detail = exception.Message });
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(await BuildDetail(order.Id, cancellationToken));
    }

    private async Task ReceiveGoods(PurchaseOrder order, IReadOnlyCollection<PurchaseReceiveLineRequest> lines, CancellationToken cancellationToken)
    {
        var quantities = new Dictionary<Guid, decimal>();
        if (lines.Count == 0)
        {
            foreach (var line in order.Lines.Where(item => item.OutstandingQuantity > 0))
                quantities[line.Id] = line.OutstandingQuantity;
        }
        else
        {
            foreach (var line in lines)
                quantities[line.LineId] = line.Quantity;
        }

        var now = DateTimeOffset.UtcNow;
        var before = order.Lines.ToDictionary(line => line.Id, line => line.ReceivedQuantity);
        order.Receive(quantities, now);

        foreach (var line in order.Lines)
        {
            var delta = line.ReceivedQuantity - before[line.Id];
            if (delta <= 0) continue;
            var balance = await db.InventoryBalances.SingleOrDefaultAsync(item => item.WarehouseId == order.WarehouseId && item.ProductId == line.ProductId, cancellationToken);
            if (balance is null)
            {
                balance = new InventoryBalance(order.WarehouseId, line.ProductId);
                db.InventoryBalances.Add(balance);
            }
            balance.ApplyReceipt(delta, line.UnitCost, now);
            db.StockMovements.Add(new StockMovement(order.WarehouseId, line.ProductId, "PurchaseReceipt", delta, line.UnitCost, order.PurchaseOrderNumber, "Goods receipt"));
        }
    }

    private async Task<object?> BuildDetail(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.PurchaseOrders.AsNoTracking().Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (order is null) return null;
        var supplier = await db.Suppliers.AsNoTracking().SingleOrDefaultAsync(item => item.Id == order.SupplierId, cancellationToken);
        var warehouse = await db.Warehouses.AsNoTracking().SingleOrDefaultAsync(item => item.Id == order.WarehouseId, cancellationToken);
        var productIds = order.Lines.Select(line => line.ProductId).ToArray();
        var products = await db.Products.AsNoTracking().Where(item => productIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, cancellationToken);
        return new
        {
            order.Id,
            order.PurchaseOrderNumber,
            order.SupplierId,
            Supplier = supplier?.Name,
            order.WarehouseId,
            Warehouse = warehouse?.Code,
            Status = order.Status.ToString(),
            order.Currency,
            order.TaxRate,
            order.Subtotal,
            order.TaxTotal,
            order.GrandTotal,
            order.Notes,
            order.OrderedAt,
            order.ExpectedAt,
            order.ReceivedAt,
            order.UpdatedAt,
            Lines = order.Lines.Select(line => new
            {
                line.Id,
                line.ProductId,
                Product = products.TryGetValue(line.ProductId, out var product) ? product.Name : "Unknown",
                line.Quantity,
                line.ReceivedQuantity,
                OutstandingQuantity = line.Quantity - line.ReceivedQuantity,
                line.UnitCost,
                line.LineTotal
            })
        };
    }
}

public sealed record CreatePurchaseOrderLineRequest(Guid ProductId, decimal Quantity, decimal? UnitCost = null);
public sealed record CreatePurchaseOrderRequest(Guid SupplierId, Guid WarehouseId, string? Currency, string? Notes, DateTimeOffset? ExpectedAt, IReadOnlyCollection<CreatePurchaseOrderLineRequest> Lines);
public sealed record PurchaseReceiveLineRequest(Guid LineId, decimal Quantity);
public sealed record PurchaseTransitionRequest(string Action, IReadOnlyCollection<PurchaseReceiveLineRequest>? Lines = null);
