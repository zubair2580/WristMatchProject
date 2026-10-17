using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/admin/overview")]
public sealed class AdminOverviewController(WristMatchDbContext db, UserManager<AdminUser> users, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var products = await db.Products.AsNoTracking().Where(product => product.IsActive).Select(product => new { product.Price, product.Material, product.TryOnReady }).ToArrayAsync(cancellationToken);
        var adminUsers = await users.Users.CountAsync(user => user.IsActive, cancellationToken);
        var customers = await db.CustomerProfiles.CountAsync(cancellationToken);
        var suppliers = await db.Suppliers.CountAsync(supplier => supplier.IsActive, cancellationToken);
        var priceBands = new[]
        {
            new { label = "Under 15k", value = products.Count(product => product.Price < 15000) },
            new { label = "15k–20k", value = products.Count(product => product.Price >= 15000 && product.Price < 20000) },
            new { label = "20k–30k", value = products.Count(product => product.Price >= 20000 && product.Price < 30000) },
            new { label = "30k+", value = products.Count(product => product.Price >= 30000) }
        };
        var materials = products.GroupBy(product => product.Material).OrderByDescending(group => group.Count()).Take(5).Select(group => new { label = group.Key, value = group.Count() }).ToArray();

        var commerce = await BuildCommerce(cancellationToken);
        var providerMode = configuration["TryOn:ProviderMode"] ?? "Not configured";

        return Ok(new
        {
            activeProducts = products.Length,
            readyProducts = products.Count(product => product.TryOnReady),
            adminUsers,
            customers,
            suppliers,
            averageProductPrice = products.Length == 0 ? 0 : products.Average(product => product.Price),
            priceBands,
            materials,
            providerMode,
            commerce
        });
    }

    private async Task<object> BuildCommerce(CancellationToken cancellationToken)
    {
        var days = 14;
        var today = DateTimeOffset.UtcNow.Date;
        var from = today.AddDays(-(days - 1));
        var fromOffset = new DateTimeOffset(from, TimeSpan.Zero);

        var postedStatuses = new[] { SalesOrderStatus.Confirmed, SalesOrderStatus.Processing, SalesOrderStatus.Completed };
        var orders = await db.SalesOrders.AsNoTracking()
            .Where(order => postedStatuses.Contains(order.Status) && order.PlacedAt >= fromOffset)
            .Select(order => new { order.PlacedAt, order.GrandTotal })
            .ToArrayAsync(cancellationToken);

        var payments = await db.Payments.AsNoTracking()
            .Where(payment => payment.Status == PaymentRecordStatus.Confirmed && payment.ReceivedAt >= fromOffset)
            .Select(payment => new { payment.ReceivedAt, payment.Amount })
            .ToArrayAsync(cancellationToken);

        var openOrders = await db.SalesOrders.CountAsync(
            order => order.Status == SalesOrderStatus.Confirmed || order.Status == SalesOrderStatus.Processing,
            cancellationToken);
        var outstandingAr = await db.SalesInvoices
            .Where(invoice => invoice.Status == SalesInvoiceStatus.Issued)
            .SumAsync(invoice => (decimal?)invoice.OutstandingAmount, cancellationToken) ?? 0;
        var issuedInvoices = await db.SalesInvoices.CountAsync(
            invoice => invoice.Status == SalesInvoiceStatus.Issued || invoice.Status == SalesInvoiceStatus.Paid,
            cancellationToken);

        var salesByDay = Enumerable.Range(0, days).Select(offset =>
        {
            var day = from.AddDays(offset);
            var total = orders.Where(order => order.PlacedAt.UtcDateTime.Date == day).Sum(order => order.GrandTotal);
            return new { label = day.ToString("dd MMM"), value = Math.Round(total, 2) };
        }).ToArray();

        var revenueByDay = Enumerable.Range(0, days).Select(offset =>
        {
            var day = from.AddDays(offset);
            var total = payments.Where(payment => payment.ReceivedAt.UtcDateTime.Date == day).Sum(payment => payment.Amount);
            return new { label = day.ToString("dd MMM"), value = Math.Round(total, 2) };
        }).ToArray();

        var salesTotal = Math.Round(salesByDay.Sum(item => item.value), 2);
        var revenueTotal = Math.Round(revenueByDay.Sum(item => item.value), 2);

        return new
        {
            periodDays = days,
            salesTotal,
            revenueTotal,
            outstandingAr = Math.Round(outstandingAr, 2),
            openOrders,
            issuedInvoices,
            hasSales = salesTotal > 0,
            hasRevenue = revenueTotal > 0,
            salesByDay,
            revenueByDay
        };
    }
}
