using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,OperationsManager,CatalogManager")]
[ApiController]
[Route("api/v1/admin/invoices")]
public sealed class AdminInvoicesController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = from invoice in db.SalesInvoices.AsNoTracking()
                    join customer in db.CustomerProfiles.AsNoTracking() on invoice.CustomerId equals customer.Id
                    join order in db.SalesOrders.AsNoTracking() on invoice.SalesOrderId equals order.Id
                    select new
                    {
                        invoice.Id,
                        Invoice = invoice.InvoiceNumber,
                        Order = order.OrderNumber,
                        Customer = customer.FirstName + " " + customer.LastName,
                        Issued = invoice.InvoiceDate,
                        Due = invoice.DueDate,
                        Total = invoice.GrandTotal,
                        Balance = invoice.OutstandingAmount,
                        Status = invoice.Status.ToString(),
                        invoice.Currency
                    };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item =>
                item.Invoice.ToLower().Contains(term) ||
                item.Order.ToLower().Contains(term) ||
                item.Customer.ToLower().Contains(term) ||
                item.Status.ToLower().Contains(term));
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(item => item.Issued).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);
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
    public async Task<IActionResult> Create(CreateSalesInvoiceRequest request, CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders.Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == request.SalesOrderId, cancellationToken);
        if (order is null)
            return BadRequest(new { code = "ORDER_NOT_FOUND", detail = "Choose a valid sales order." });
        if (order.Status == SalesOrderStatus.Cancelled)
            return BadRequest(new { code = "ORDER_CANCELLED", detail = "Cannot invoice a cancelled order." });
        if (order.Lines.Count == 0)
            return BadRequest(new { code = "ORDER_EMPTY", detail = "Order has no lines to invoice." });

        var existing = await db.SalesInvoices.AnyAsync(
            item => item.SalesOrderId == order.Id && item.Status != SalesInvoiceStatus.Cancelled,
            cancellationToken);
        if (existing)
            return Conflict(new { code = "INVOICE_EXISTS", detail = "This order already has an active invoice." });

        var now = DateTimeOffset.UtcNow;
        var due = request.DueDate ?? now.AddDays(14);
        var number = $"INV-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var invoice = new SalesInvoice(number, order.Id, order.CustomerId, order.Currency, now, due);
        invoice.SetNotes(request.Notes);
        invoice.ReplaceLines(order.Lines.Select(line => (line.ProductId, line.Quantity, line.UnitPrice, line.TaxAmount, line.LineTotal)));
        db.SalesInvoices.Add(invoice);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/invoices/{invoice.Id}", await BuildDetail(invoice.Id, cancellationToken));
    }

    [HttpPost("{id:guid}/transitions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transition(Guid id, InvoiceTransitionRequest request, CancellationToken cancellationToken)
    {
        var invoice = await db.SalesInvoices.Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (invoice is null) return NotFound();

        try
        {
            switch (request.Action.Trim().ToLowerInvariant())
            {
                case "issue":
                    invoice.Issue();
                    break;
                case "pay":
                    await RecordPayment(invoice, request, cancellationToken);
                    break;
                case "cancel":
                    var hasPayments = await db.Payments.AnyAsync(
                        payment => payment.SalesInvoiceId == invoice.Id && payment.Status == PaymentRecordStatus.Confirmed,
                        cancellationToken);
                    invoice.Cancel(hasPayments);
                    break;
                default:
                    return BadRequest(new { code = "UNKNOWN_ACTION", detail = "Use issue, pay, or cancel." });
            }
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { code = "INVOICE_RULE", detail = exception.Message });
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(await BuildDetail(invoice.Id, cancellationToken));
    }

    private async Task RecordPayment(SalesInvoice invoice, InvoiceTransitionRequest request, CancellationToken cancellationToken)
    {
        var amount = request.Amount ?? invoice.OutstandingAmount;
        var method = string.IsNullOrWhiteSpace(request.Method) ? "BankTransfer" : request.Method.Trim();
        var number = $"PAY-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var payment = invoice.RecordPayment(number, method, amount, DateTimeOffset.UtcNow);
        db.Payments.Add(payment);

        if (invoice.Status == SalesInvoiceStatus.Paid)
        {
            var order = await db.SalesOrders.SingleOrDefaultAsync(item => item.Id == invoice.SalesOrderId, cancellationToken);
            order?.MarkPaid();
        }
    }

    private async Task<object?> BuildDetail(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await db.SalesInvoices.AsNoTracking()
            .Include(item => item.Lines)
            .Include(item => item.Payments)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (invoice is null) return null;
        var customer = await db.CustomerProfiles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == invoice.CustomerId, cancellationToken);
        var order = await db.SalesOrders.AsNoTracking().SingleOrDefaultAsync(item => item.Id == invoice.SalesOrderId, cancellationToken);
        var productIds = invoice.Lines.Select(line => line.ProductId).ToArray();
        var products = await db.Products.AsNoTracking().Where(item => productIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, cancellationToken);
        return new
        {
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.SalesOrderId,
            OrderNumber = order?.OrderNumber,
            invoice.CustomerId,
            Customer = $"{customer?.FirstName} {customer?.LastName}".Trim(),
            Status = invoice.Status.ToString(),
            invoice.Currency,
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.Subtotal,
            invoice.TaxTotal,
            invoice.GrandTotal,
            invoice.OutstandingAmount,
            invoice.Notes,
            invoice.UpdatedAt,
            Lines = invoice.Lines.Select(line => new
            {
                line.Id,
                line.ProductId,
                Product = products.TryGetValue(line.ProductId, out var product) ? product.Name : "Unknown",
                line.Quantity,
                line.UnitPrice,
                line.TaxAmount,
                line.LineTotal
            }),
            Payments = invoice.Payments
                .OrderByDescending(payment => payment.ReceivedAt)
                .Select(payment => new
                {
                    payment.Id,
                    payment.PaymentNumber,
                    payment.Method,
                    Status = payment.Status.ToString(),
                    payment.Amount,
                    payment.Currency,
                    payment.ReceivedAt
                })
        };
    }
}

public sealed record CreateSalesInvoiceRequest(Guid SalesOrderId, string? Notes, DateTimeOffset? DueDate);
public sealed record InvoiceTransitionRequest(string Action, decimal? Amount = null, string? Method = null);
