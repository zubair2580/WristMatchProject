using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,OperationsManager,PurchasingManager")]
[ApiController]
[Route("api/v1/admin/customers")]
public sealed class AdminCustomersController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] bool includeInactive = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.CustomerProfiles.AsNoTracking();
        if (!includeInactive)
            query = query.Where(item => item.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(item => item.CustomerNumber.Contains(search) || item.FirstName.Contains(search) || item.LastName.Contains(search) || item.Email.Contains(search));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(item => item.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(item => new CustomerResponse(item.Id, item.CustomerNumber, item.FirstName, item.LastName, item.Email, item.Phone, item.IsGuest, item.MarketingConsent, item.IsActive, item.CreatedAt, Convert.ToBase64String(item.RowVersion)))
            .ToArrayAsync(cancellationToken);
        return Ok(new { items, page, pageSize, total, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerRequest request, CancellationToken cancellationToken)
    {
        var validation = Validate(request);
        if (validation is not null)
            return validation;
        var number = $"CUS-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        var customer = new CustomerProfile(number, request.FirstName.Trim(), request.LastName.Trim(), request.Email.Trim(), request.Phone.Trim(), request.IsGuest);
        customer.Update(request.FirstName.Trim(), request.LastName.Trim(), request.Email.Trim(), request.Phone.Trim(), request.MarketingConsent, request.IsGuest, true);
        db.CustomerProfiles.Add(customer);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/customers/{customer.Id}", ToResponse(customer));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, CustomerRequest request, CancellationToken cancellationToken)
    {
        var validation = Validate(request);
        if (validation is not null)
            return validation;
        var customer = await db.CustomerProfiles.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (customer is null)
            return NotFound();
        try { db.Entry(customer).Property(item => item.RowVersion).OriginalValue = Convert.FromBase64String(request.RowVersion); }
        catch { return Conflict(new { code = "ROW_VERSION_INVALID", detail = "Refresh this customer and try again." }); }
        customer.Update(request.FirstName.Trim(), request.LastName.Trim(), request.Email.Trim(), request.Phone.Trim(), request.MarketingConsent, request.IsGuest, request.IsActive);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Conflict(new { code = "CONCURRENCY_CONFLICT", detail = "Another administrator changed this customer." }); }
        return Ok(ToResponse(customer));
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var customer = await db.CustomerProfiles.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (customer is null)
            return NotFound();
        customer.Deactivate();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static CustomerResponse ToResponse(CustomerProfile customer) => new(customer.Id, customer.CustomerNumber, customer.FirstName, customer.LastName, customer.Email, customer.Phone, customer.IsGuest, customer.MarketingConsent, customer.IsActive, customer.CreatedAt, Convert.ToBase64String(customer.RowVersion));
    private static ActionResult? Validate(CustomerRequest request) => string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || !request.Email.Contains('@') || string.IsNullOrWhiteSpace(request.Phone) ? new BadRequestObjectResult(new { code = "INVALID_CUSTOMER", detail = "First name, last name, valid email, and phone are required." }) : null;
}

[Authorize(Roles = "SuperAdmin,PurchasingManager,OperationsManager")]
[ApiController]
[Route("api/v1/admin/suppliers")]
public sealed class AdminSuppliersController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Suppliers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(item => item.SupplierNumber.Contains(search) || item.Name.Contains(search) || item.Email.Contains(search));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(item => item.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(item => new SupplierResponse(item.Id, item.SupplierNumber, item.Name, item.Email, item.Phone, item.Currency, item.PaymentTermsDays, item.IsActive, item.UpdatedAt, Convert.ToBase64String(item.RowVersion)))
            .ToArrayAsync(cancellationToken);
        return Ok(new { items, page, pageSize, total, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierRequest request, CancellationToken cancellationToken)
    {
        var number = $"SUP-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        var supplier = new Supplier(number, request.Name.Trim(), request.Email.Trim(), request.Phone.Trim(), request.Currency.ToUpperInvariant());
        supplier.Update(request.Name.Trim(), request.Email.Trim(), request.Phone.Trim(), request.Currency.ToUpperInvariant(), request.PaymentTermsDays, request.IsActive);
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/v1/admin/suppliers/{supplier.Id}", ToResponse(supplier));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, SupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (supplier is null)
            return NotFound();
        try { db.Entry(supplier).Property(item => item.RowVersion).OriginalValue = Convert.FromBase64String(request.RowVersion); }
        catch { return Conflict(new { code = "ROW_VERSION_INVALID", detail = "Refresh this supplier and try again." }); }
        supplier.Update(request.Name.Trim(), request.Email.Trim(), request.Phone.Trim(), request.Currency.ToUpperInvariant(), request.PaymentTermsDays, request.IsActive);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Conflict(new { code = "CONCURRENCY_CONFLICT", detail = "Another administrator changed this supplier." }); }
        return Ok(ToResponse(supplier));
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (supplier is null)
            return NotFound();
        supplier.Update(supplier.Name, supplier.Email, supplier.Phone, supplier.Currency, supplier.PaymentTermsDays, false);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static SupplierResponse ToResponse(Supplier item) => new(item.Id, item.SupplierNumber, item.Name, item.Email, item.Phone, item.Currency, item.PaymentTermsDays, item.IsActive, item.UpdatedAt, Convert.ToBase64String(item.RowVersion));
}

public sealed record CustomerRequest(string FirstName, string LastName, string Email, string Phone, bool MarketingConsent, bool IsGuest, bool IsActive = true, string RowVersion = "");
public sealed record CustomerResponse(Guid Id, string CustomerNumber, string FirstName, string LastName, string Email, string Phone, bool IsGuest, bool MarketingConsent, bool IsActive, DateTimeOffset CreatedAt, string RowVersion);
public sealed record SupplierRequest(string Name, string Email, string Phone, string Currency, int PaymentTermsDays, bool IsActive, string RowVersion = "");
public sealed record SupplierResponse(Guid Id, string SupplierNumber, string Name, string Email, string Phone, string Currency, int PaymentTermsDays, bool IsActive, DateTimeOffset UpdatedAt, string RowVersion);
