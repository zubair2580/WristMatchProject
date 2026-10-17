using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/admin/demo-modules")]
public sealed class AdminDemoModulesController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet("{module}")]
    public async Task<IActionResult> Get(string module, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var sql = module.ToLowerInvariant() switch
        {
            "orders" => "SELECT TOP (100) o.OrderNumber AS [Order], CONCAT(c.FirstName, ' ', c.LastName) AS Customer, o.PlacedAt AS Placed, o.GrandTotal AS Total, o.PaymentStatus AS Payment, o.FulfilmentStatus AS Fulfilment FROM SalesOrders o LEFT JOIN CustomerProfiles c ON c.Id = o.CustomerId ORDER BY o.PlacedAt DESC",
            "invoices" => "SELECT TOP (100) i.InvoiceNumber AS Invoice, CONCAT(c.FirstName, ' ', c.LastName) AS Customer, i.InvoiceDate AS Issued, i.DueDate AS Due, i.GrandTotal AS Total, i.OutstandingAmount AS Balance, i.Status FROM SalesInvoices i LEFT JOIN CustomerProfiles c ON c.Id = i.CustomerId ORDER BY i.InvoiceDate DESC",
            "stock" => "SELECT TOP (100) CONCAT(p.SKU, ' / ', p.Name) AS Product, w.Code AS Warehouse, s.MovementType AS Movement, s.Quantity, s.UnitCost, s.TotalCost AS Value, s.ReferenceNumber AS Reference, s.OccurredAt FROM StockMovements s LEFT JOIN Products p ON p.Id = s.ProductId LEFT JOIN Warehouses w ON w.Id = s.WarehouseId ORDER BY s.OccurredAt DESC",
            "purchases" => "SELECT TOP (100) po.PurchaseOrderNumber AS [Purchase order], s.Name AS Supplier, w.Code AS Warehouse, po.OrderedAt AS Ordered, po.ExpectedAt AS Expected, po.GrandTotal AS Total, po.Status FROM PurchaseOrders po LEFT JOIN Suppliers s ON s.Id = po.SupplierId LEFT JOIN Warehouses w ON w.Id = po.WarehouseId ORDER BY po.OrderedAt DESC",
            "accounts" => "SELECT TOP (100) Code, Name AS Account, AccountType AS Type, NormalBalance AS [Normal balance], CurrentBalance AS [Current balance], IIF(IsActive = 1, 'Active', 'Archived') AS Status FROM Accounts ORDER BY Code",
            "reports" => "SELECT TOP (100) Name AS Report, Module, OwnerEmail AS Owner, LastRunAt AS [Last run], Format, Status FROM ReportDefinitions ORDER BY CreatedAt DESC",
            "tryons" => "SELECT TOP (100) j.Id AS Job, p.Name AS Product, j.CreatedAt AS Created, j.State, j.ProviderTaskId AS [Provider task], DATEDIFF(second, j.CreatedAt, COALESCE(j.CompletedAt, SYSUTCDATETIME())) AS [Duration seconds] FROM TryOnJobs j LEFT JOIN Products p ON p.Id = j.ProductId ORDER BY j.CreatedAt DESC",
            _ => null
        };
        if (sql is null)
            return NotFound();
        var rows = new List<Dictionary<string, object?>>();
        await using var connection = db.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < reader.FieldCount; index++)
                row[reader.GetName(index)] = reader.IsDBNull(index) ? null : reader.GetValue(index);
            if (string.IsNullOrWhiteSpace(search) || row.Values.Any(value => value?.ToString()?.Contains(search, StringComparison.OrdinalIgnoreCase) == true))
                rows.Add(row);
        }
        return Ok(new { items = rows, page = 1, pageSize = rows.Count, total = rows.Count, totalPages = 1 });
    }
}
