using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,OperationsManager,CatalogManager")]
[ApiController]
[Route("api/v1/admin/orders")]
public sealed class AdminOrdersController(WristMatchDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = from order in db.SalesOrders.AsNoTracking()
                    join customer in db.CustomerProfiles.AsNoTracking() on order.CustomerId equals customer.Id
                    select new
                    {
                        order.Id,
                        Order = order.OrderNumber,
                        Customer = customer.FirstName + " " + customer.LastName,
                        Placed = order.PlacedAt,
                        Total = order.GrandTotal,
                        Status = order.Status.ToString(),
                        Payment = order.PaymentStatus.ToString(),
                        Fulfilment = order.FulfilmentStatus.ToString(),
                        order.Currency
                    };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item => item.Order.ToLower().Contains(term) || item.Customer.ToLower().Contains(term) || item.Status.ToLower().Contains(term));
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(item => item.Placed).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);
        return Ok(new { items, page, pageSize, total, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)) });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders.AsNoTracking().Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (order is null) return NotFound();
        var customer = await db.CustomerProfiles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == order.CustomerId, cancellationToken);
        var warehouse = await db.Warehouses.AsNoTracking().SingleOrDefaultAsync(item => item.Id == order.WarehouseId, cancellationToken);
        var productIds = order.Lines.Select(line => line.ProductId).ToArray();
        var products = await db.Products.AsNoTracking().Where(item => productIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, cancellationToken);
        return Ok(ToDetail(order, customer?.FirstName + " " + customer?.LastName, warehouse?.Code, products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSalesOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
            return BadRequest(new { code = "LINES_REQUIRED", detail = "Add at least one order line." });
        var customer = await db.CustomerProfiles.SingleOrDefaultAsync(item => item.Id == request.CustomerId && item.IsActive, cancellationToken);
        var warehouse = await db.Warehouses.SingleOrDefaultAsync(item => item.Id == request.WarehouseId && item.IsActive, cancellationToken);
        if (customer is null || warehouse is null)
            return BadRequest(new { code = "INVALID_REFERENCES", detail = "Choose an active customer and warehouse." });

        var productIds = request.Lines.Select(line => line.ProductId).Distinct().ToArray();
        var products = await db.Products.Where(item => productIds.Contains(item.Id) && item.IsActive).ToDictionaryAsync(item => item.Id, cancellationToken);
        if (products.Count != productIds.Length)
            return BadRequest(new { code = "INVALID_PRODUCT", detail = "One or more products are inactive or missing." });

        var taxRate = configuration.GetValue("Commerce:DefaultTaxRate", 0.10m);
        var orderNumber = $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var order = new SalesOrder(orderNumber, request.CustomerId, request.WarehouseId, request.Currency ?? "PKR", taxRate);
        order.SetNotes(request.Notes);
        order.ReplaceLines(request.Lines.Select(line =>
        {
            var product = products[line.ProductId];
            var unitPrice = line.UnitPrice ?? product.Price;
            return (line.ProductId, line.Quantity, unitPrice);
        }));
        db.SalesOrders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/orders/{order.Id}", await GetDetailResponse(order.Id, cancellationToken));
    }

    [HttpPost("{id:guid}/transitions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(Guid id, OrderTransitionRequest request, CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders.Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (order is null) return NotFound();

        try
        {
            switch (request.Action.Trim().ToLowerInvariant())
            {
                case "confirm":
                    await ReserveStock(order, cancellationToken);
                    order.Confirm();
                    break;
                case "process":
                    order.StartProcessing();
                    break;
                case "complete":
                    await FulfilStock(order, cancellationToken);
                    order.Complete();
                    break;
                case "cancel":
                    if (order.HasStockReservation)
                        await ReleaseStock(order, cancellationToken);
                    order.Cancel();
                    break;
                case "mark-paid":
                    order.MarkPaid();
                    break;
                default:
                    return BadRequest(new { code = "UNKNOWN_ACTION", detail = "Use confirm, process, complete, cancel, or mark-paid." });
            }
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { code = "ORDER_RULE", detail = exception.Message });
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(await GetDetailResponse(order.Id, cancellationToken));
    }

    private async Task ReserveStock(SalesOrder order, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var warehouse = await db.Warehouses.AsNoTracking().SingleOrDefaultAsync(item => item.Id == order.WarehouseId, cancellationToken);
        foreach (var line in order.Lines)
        {
            var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(item => item.Id == line.ProductId, cancellationToken);
            var balance = await db.InventoryBalances.SingleOrDefaultAsync(item => item.WarehouseId == order.WarehouseId && item.ProductId == line.ProductId, cancellationToken);
            if (balance is null)
            {
                var productName = product?.Name ?? "this product";
                var warehouseCode = warehouse?.Code ?? "the order warehouse";
                throw new InvalidOperationException($"No stock balance for {productName} at {warehouseCode}. Open Stock → Stock adjustment and add on-hand qty for this warehouse, then try Confirm & reserve again.");
            }
            try
            {
                balance.Reserve(line.Quantity, now);
            }
            catch (InvalidOperationException exception) when (exception.Message.Contains("Insufficient", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Not enough available stock for {product?.Name ?? "product"} at {warehouse?.Code ?? "warehouse"} (need {line.Quantity}, available {balance.Available}).");
            }
            db.StockMovements.Add(new StockMovement(order.WarehouseId, line.ProductId, "Reservation", line.Quantity, balance.AverageCost, order.OrderNumber, "Order confirmation reserve"));
        }
    }

    private async Task ReleaseStock(SalesOrder order, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var line in order.Lines)
        {
            var balance = await db.InventoryBalances.SingleOrDefaultAsync(item => item.WarehouseId == order.WarehouseId && item.ProductId == line.ProductId, cancellationToken)
                ?? throw new InvalidOperationException($"No stock balance to release for product {line.ProductId:N}.");
            balance.Release(line.Quantity, now);
            db.StockMovements.Add(new StockMovement(order.WarehouseId, line.ProductId, "Release", -line.Quantity, balance.AverageCost, order.OrderNumber, "Order cancellation release"));
        }
    }

    private async Task FulfilStock(SalesOrder order, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var line in order.Lines)
        {
            var balance = await db.InventoryBalances.SingleOrDefaultAsync(item => item.WarehouseId == order.WarehouseId && item.ProductId == line.ProductId, cancellationToken)
                ?? throw new InvalidOperationException($"No stock balance to fulfil for product {line.ProductId:N}.");
            balance.FulfilReserved(line.Quantity, now);
            db.StockMovements.Add(new StockMovement(order.WarehouseId, line.ProductId, "Sale", -line.Quantity, balance.AverageCost, order.OrderNumber, "Order fulfilment"));
        }
    }

    private async Task<object> GetDetailResponse(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders.AsNoTracking().Include(item => item.Lines).SingleAsync(item => item.Id == id, cancellationToken);
        var customer = await db.CustomerProfiles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == order.CustomerId, cancellationToken);
        var warehouse = await db.Warehouses.AsNoTracking().SingleOrDefaultAsync(item => item.Id == order.WarehouseId, cancellationToken);
        var productIds = order.Lines.Select(line => line.ProductId).ToArray();
        var products = await db.Products.AsNoTracking().Where(item => productIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, cancellationToken);
        return ToDetail(order, $"{customer?.FirstName} {customer?.LastName}".Trim(), warehouse?.Code, products);
    }

    private static object ToDetail(SalesOrder order, string? customerName, string? warehouseCode, IReadOnlyDictionary<Guid, Product> products) => new
    {
        order.Id,
        order.OrderNumber,
        order.CustomerId,
        Customer = customerName,
        order.WarehouseId,
        Warehouse = warehouseCode,
        Status = order.Status.ToString(),
        PaymentStatus = order.PaymentStatus.ToString(),
        FulfilmentStatus = order.FulfilmentStatus.ToString(),
        order.Currency,
        order.TaxRate,
        order.Subtotal,
        order.TaxTotal,
        order.GrandTotal,
        order.Notes,
        order.PlacedAt,
        order.UpdatedAt,
        Lines = order.Lines.Select(line => new
        {
            line.Id,
            line.ProductId,
            Product = products.TryGetValue(line.ProductId, out var product) ? product.Name : "Unknown",
            line.Quantity,
            line.UnitPrice,
            line.TaxAmount,
            line.LineTotal
        })
    };
}

public sealed record CreateSalesOrderLineRequest(Guid ProductId, decimal Quantity, decimal? UnitPrice = null);
public sealed record CreateSalesOrderRequest(Guid CustomerId, Guid WarehouseId, string? Currency, string? Notes, IReadOnlyCollection<CreateSalesOrderLineRequest> Lines);
public sealed record OrderTransitionRequest(string Action);
