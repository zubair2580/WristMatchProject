using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[ApiController]
[Route("api/v1/checkout")]
public sealed class CheckoutController(WristMatchDbContext db, IConfiguration configuration) : ControllerBase
{
    private const string CustomerCookie = "wristmatch.customer";

    [HttpPost("orders")]
    public async Task<IActionResult> PlaceOrder(CheckoutOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Lines is null || request.Lines.Count == 0)
            return BadRequest(new { code = "LINES_REQUIRED", detail = "Your bag is empty." });
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || !request.Email.Contains('@') || string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest(new { code = "CUSTOMER_REQUIRED", detail = "First name, last name, email, and phone are required." });
        if (request.ShippingAddress is null || string.IsNullOrWhiteSpace(request.ShippingAddress.AddressLine1) || string.IsNullOrWhiteSpace(request.ShippingAddress.City))
            return BadRequest(new { code = "ADDRESS_REQUIRED", detail = "Shipping address line and city are required." });

        var paymentMethod = NormalizePayment(request.PaymentMethod);
        var productIds = request.Lines.Select(line => line.ProductId).Distinct().ToArray();
        var products = await db.Products.Where(item => productIds.Contains(item.Id) && item.IsActive).ToDictionaryAsync(item => item.Id, cancellationToken);
        if (products.Count != productIds.Length)
            return BadRequest(new { code = "INVALID_PRODUCT", detail = "One or more products are unavailable." });

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0 || line.Quantity > 9)
                return BadRequest(new { code = "INVALID_QTY", detail = "Each line quantity must be between 1 and 9." });
        }

        var required = request.Lines.GroupBy(line => line.ProductId).ToDictionary(group => group.Key, group => group.Sum(line => line.Quantity));
        var warehouses = await db.Warehouses.Where(item => item.IsActive).OrderBy(item => item.Code).ToListAsync(cancellationToken);
        if (warehouses.Count == 0)
            return BadRequest(new { code = "NO_WAREHOUSE", detail = "No active warehouse is configured." });

        Warehouse? warehouse = null;
        foreach (var candidate in warehouses)
        {
            var balances = await db.InventoryBalances.AsNoTracking()
                .Where(item => item.WarehouseId == candidate.Id && productIds.Contains(item.ProductId))
                .ToListAsync(cancellationToken);
            var ok = required.All(pair =>
            {
                var balance = balances.FirstOrDefault(item => item.ProductId == pair.Key);
                return balance is not null && balance.Available >= pair.Value;
            });
            if (ok)
            {
                warehouse = candidate;
                break;
            }
        }
        warehouse ??= warehouses[0];

        var email = request.Email.Trim().ToLowerInvariant();
        var customer = await db.CustomerProfiles.SingleOrDefaultAsync(item => item.Email.ToLower() == email, cancellationToken);
        if (customer is null)
        {
            customer = new CustomerProfile(
                $"CUS-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                request.FirstName.Trim(),
                request.LastName.Trim(),
                email,
                request.Phone.Trim(),
                isGuest: true);
            db.CustomerProfiles.Add(customer);
        }
        else
        {
            customer.Update(request.FirstName.Trim(), request.LastName.Trim(), email, request.Phone.Trim(), customer.MarketingConsent, customer.IsGuest, true);
        }

        if (request.CreateAccount && !string.IsNullOrWhiteSpace(request.Password) && request.Password.Length >= 8)
            customer.SetPasswordHash(HashPassword(request.Password));

        await db.SaveChangesAsync(cancellationToken);

        var recipient = string.IsNullOrWhiteSpace(request.ShippingAddress.RecipientName)
            ? $"{request.FirstName.Trim()} {request.LastName.Trim()}"
            : request.ShippingAddress.RecipientName.Trim();

        db.CustomerAddresses.Add(new CustomerAddress(
            customer.Id,
            "Shipping",
            recipient,
            string.IsNullOrWhiteSpace(request.ShippingAddress.Phone) ? request.Phone.Trim() : request.ShippingAddress.Phone.Trim(),
            request.ShippingAddress.AddressLine1.Trim(),
            string.IsNullOrWhiteSpace(request.ShippingAddress.AddressLine2) ? null : request.ShippingAddress.AddressLine2.Trim(),
            request.ShippingAddress.City.Trim(),
            string.IsNullOrWhiteSpace(request.ShippingAddress.StateProvince) ? null : request.ShippingAddress.StateProvince.Trim(),
            string.IsNullOrWhiteSpace(request.ShippingAddress.PostalCode) ? null : request.ShippingAddress.PostalCode.Trim(),
            request.ShippingAddress.CountryCode ?? "PK",
            true));

        var taxRate = configuration.GetValue("Commerce:DefaultTaxRate", 0.10m);
        var orderNumber = $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var order = new SalesOrder(orderNumber, customer.Id, warehouse.Id, "PKR", taxRate);
        var shippingNote = BuildShippingNotes(paymentMethod, request.Notes, request.ShippingAddress, recipient);
        order.SetNotes(shippingNote);
        order.ReplaceLines(request.Lines.Select(line =>
        {
            var product = products[line.ProductId];
            return (line.ProductId, (decimal)line.Quantity, product.Price);
        }));

        db.SalesOrders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        var confirmed = false;
        try
        {
            await ReserveStock(order, cancellationToken);
            order.Confirm();
            confirmed = true;
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Leave as draft when stock cannot be reserved; still visible in admin.
        }

        IssueCustomerCookie(customer.Id);

        var subtotal = order.Subtotal;
        var shipping = subtotal >= 50000 ? 0m : 500m;
        var response = ToCheckoutResponse(order, customer, paymentMethod, shipping, confirmed, request.ShippingAddress, recipient);
        return Created($"/api/v1/checkout/orders/{order.Id}", response);
    }

    [HttpGet("orders/{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders.AsNoTracking().Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (order is null) return NotFound();
        var customer = await db.CustomerProfiles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == order.CustomerId, cancellationToken);
        if (customer is null) return NotFound();
        if (!CanAccessCustomer(customer.Id))
            return Unauthorized(new { code = "LOGIN_REQUIRED", detail = "Sign in to view this order." });

        var paymentMethod = ExtractPaymentMethod(order.Notes);
        var shipping = order.Subtotal >= 50000 ? 0m : 500m;
        var address = await db.CustomerAddresses.AsNoTracking()
            .Where(item => item.CustomerId == customer.Id && item.Type == "Shipping")
            .OrderByDescending(item => item.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(ToCheckoutResponse(order, customer, paymentMethod, shipping, order.Status != SalesOrderStatus.Draft, address is null ? null : new CheckoutAddressRequest(
            address.RecipientName, address.Phone, address.AddressLine1, address.AddressLine2, address.City, address.StateProvince, address.PostalCode, address.CountryCode), address?.RecipientName ?? $"{customer.FirstName} {customer.LastName}"));
    }

    [HttpPost("orders/{id:guid}/pay")]
    public async Task<IActionResult> CompletePayment(Guid id, CheckoutPayRequest request, CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders.Include(item => item.Lines).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (order is null) return NotFound();
        if (!CanAccessCustomer(order.CustomerId))
            return Unauthorized(new { code = "LOGIN_REQUIRED", detail = "Sign in to complete payment." });

        var method = NormalizePayment(request.PaymentMethod ?? ExtractPaymentMethod(order.Notes));
        if (method is not ("card" or "bank"))
            return BadRequest(new { code = "PAYMENT_METHOD", detail = "Only card or bank payments can be completed online." });

        if (request.Success == false)
            return Ok(new { order.Id, order.OrderNumber, PaymentStatus = order.PaymentStatus.ToString(), paid = false, detail = "Payment was cancelled." });

        try
        {
            if (order.Status == SalesOrderStatus.Draft)
            {
                await ReserveStock(order, cancellationToken);
                order.Confirm();
            }
            order.MarkPaid();
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { code = "PAYMENT_RULE", detail = exception.Message });
        }

        var customer = await db.CustomerProfiles.AsNoTracking().SingleAsync(item => item.Id == order.CustomerId, cancellationToken);
        var shipping = order.Subtotal >= 50000 ? 0m : 500m;
        return Ok(ToCheckoutResponse(order, customer, method, shipping, true, null, $"{customer.FirstName} {customer.LastName}"));
    }

    [HttpPost("account/register")]
    public async Task<IActionResult> Register(CustomerAuthRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || !request.Email.Contains('@') || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return BadRequest(new { code = "INVALID_ACCOUNT", detail = "Name, email, phone, and a password of at least 8 characters are required." });

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await db.CustomerProfiles.SingleOrDefaultAsync(item => item.Email.ToLower() == email, cancellationToken);
        if (existing is not null && !string.IsNullOrEmpty(existing.PasswordHash))
            return Conflict(new { code = "EMAIL_TAKEN", detail = "An account already exists for this email. Please sign in." });

        CustomerProfile customer;
        if (existing is null)
        {
            customer = new CustomerProfile($"CUS-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}", request.FirstName.Trim(), request.LastName.Trim(), email, request.Phone.Trim(), false);
            customer.SetPasswordHash(HashPassword(request.Password));
            db.CustomerProfiles.Add(customer);
        }
        else
        {
            customer = existing;
            customer.Update(request.FirstName.Trim(), request.LastName.Trim(), email, request.Phone.Trim(), false, false, true);
            customer.SetPasswordHash(HashPassword(request.Password));
        }

        await db.SaveChangesAsync(cancellationToken);
        IssueCustomerCookie(customer.Id);
        return Ok(ToAccount(customer));
    }

    [HttpPost("account/login")]
    public async Task<IActionResult> Login(CustomerLoginRequest request, CancellationToken cancellationToken)
    {
        if (!request.Email.Contains('@') || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { code = "INVALID_LOGIN", detail = "Email and password are required." });

        var email = request.Email.Trim().ToLowerInvariant();
        var customer = await db.CustomerProfiles.SingleOrDefaultAsync(item => item.Email.ToLower() == email && item.IsActive, cancellationToken);
        if (customer is null || string.IsNullOrEmpty(customer.PasswordHash) || !VerifyPassword(request.Password, customer.PasswordHash))
            return Unauthorized(new { code = "BAD_CREDENTIALS", detail = "Email or password is incorrect." });

        IssueCustomerCookie(customer.Id);
        return Ok(ToAccount(customer));
    }

    [HttpPost("account/logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(CustomerCookie);
        return NoContent();
    }

    [HttpGet("account/me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var customerId = ReadCustomerId();
        if (customerId is null) return Unauthorized();
        var customer = await db.CustomerProfiles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == customerId && item.IsActive, cancellationToken);
        if (customer is null) return Unauthorized();
        return Ok(ToAccount(customer));
    }

    [HttpGet("account/orders")]
    public async Task<IActionResult> MyOrders(CancellationToken cancellationToken)
    {
        var customerId = ReadCustomerId();
        if (customerId is null) return Unauthorized();

        var items = await db.SalesOrders.AsNoTracking()
            .Where(item => item.CustomerId == customerId)
            .OrderByDescending(item => item.PlacedAt)
            .Select(item => new
            {
                item.Id,
                item.OrderNumber,
                Status = item.Status.ToString(),
                PaymentStatus = item.PaymentStatus.ToString(),
                FulfilmentStatus = item.FulfilmentStatus.ToString(),
                item.Currency,
                item.GrandTotal,
                item.PlacedAt,
                ItemCount = item.Lines.Count
            })
            .ToArrayAsync(cancellationToken);

        return Ok(new { items });
    }

    private async Task ReserveStock(SalesOrder order, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var line in order.Lines)
        {
            var balance = await db.InventoryBalances.SingleOrDefaultAsync(item => item.WarehouseId == order.WarehouseId && item.ProductId == line.ProductId, cancellationToken)
                ?? throw new InvalidOperationException($"No stock for product {line.ProductId:N}.");
            balance.Reserve(line.Quantity, now);
            db.StockMovements.Add(new StockMovement(order.WarehouseId, line.ProductId, "Reservation", line.Quantity, balance.AverageCost, order.OrderNumber, "Storefront checkout reserve"));
        }
    }

    private void IssueCustomerCookie(Guid customerId)
    {
        Response.Cookies.Append(CustomerCookie, customerId.ToString("N"), new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/"
        });
    }

    private Guid? ReadCustomerId()
    {
        if (!Request.Cookies.TryGetValue(CustomerCookie, out var value) || !Guid.TryParseExact(value, "N", out var id))
            return null;
        return id;
    }

    private bool CanAccessCustomer(Guid customerId) => ReadCustomerId() == customerId;

    private static string NormalizePayment(string? method) => method?.Trim().ToLowerInvariant() switch
    {
        "bank" => "bank",
        "cod" => "cod",
        _ => "card"
    };

    private static string ExtractPaymentMethod(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return "card";
        if (notes.Contains("Payment: bank", StringComparison.OrdinalIgnoreCase)) return "bank";
        if (notes.Contains("Payment: cod", StringComparison.OrdinalIgnoreCase)) return "cod";
        return "card";
    }

    private static string BuildShippingNotes(string paymentMethod, string? notes, CheckoutAddressRequest address, string recipient)
    {
        var builder = new StringBuilder();
        builder.Append("Payment: ").Append(paymentMethod).AppendLine();
        builder.Append("Ship to: ").Append(recipient).Append(" | ").Append(address.Phone).AppendLine();
        builder.Append(address.AddressLine1);
        if (!string.IsNullOrWhiteSpace(address.AddressLine2)) builder.Append(", ").Append(address.AddressLine2);
        builder.Append(", ").Append(address.City);
        if (!string.IsNullOrWhiteSpace(address.StateProvince)) builder.Append(", ").Append(address.StateProvince);
        if (!string.IsNullOrWhiteSpace(address.PostalCode)) builder.Append(" ").Append(address.PostalCode);
        builder.Append(", ").Append(string.IsNullOrWhiteSpace(address.CountryCode) ? "PK" : address.CountryCode);
        if (!string.IsNullOrWhiteSpace(notes))
        {
            builder.AppendLine();
            builder.Append("Note: ").Append(notes.Trim());
        }
        return builder.ToString();
    }

    private object ToCheckoutResponse(SalesOrder order, CustomerProfile customer, string paymentMethod, decimal shipping, bool confirmed, CheckoutAddressRequest? address, string recipient)
    {
        var payUrl = paymentMethod == "card" ? $"/pay/{order.Id}" : null;
        return new
        {
            order.Id,
            order.OrderNumber,
            Status = order.Status.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            FulfilmentStatus = order.FulfilmentStatus.ToString(),
            PaymentMethod = paymentMethod,
            order.Currency,
            order.Subtotal,
            Shipping = shipping,
            order.TaxTotal,
            GrandTotal = order.GrandTotal + shipping,
            order.PlacedAt,
            Confirmed = confirmed,
            Customer = new { customer.Id, customer.CustomerNumber, customer.FirstName, customer.LastName, customer.Email, customer.Phone },
            ShippingAddress = address is null ? null : new
            {
                RecipientName = recipient,
                address.Phone,
                address.AddressLine1,
                address.AddressLine2,
                address.City,
                address.StateProvince,
                address.PostalCode,
                CountryCode = address.CountryCode ?? "PK"
            },
            PaymentRedirectUrl = payUrl,
            BankInstructions = paymentMethod == "bank" ? new
            {
                BankName = "HBL · WristMatch Collections",
                AccountTitle = "WristMatch Private Limited",
                Iban = "PK36 HABB 0000123400123456",
                Reference = order.OrderNumber
            } : null,
            CodNote = paymentMethod == "cod" ? "Pay the courier in cash when your order arrives. Our team will call to confirm." : null
        };
    }

    private static object ToAccount(CustomerProfile customer) => new
    {
        customer.Id,
        customer.CustomerNumber,
        customer.FirstName,
        customer.LastName,
        customer.Email,
        customer.Phone,
        HasPassword = !string.IsNullOrEmpty(customer.PasswordHash)
    };

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split('$');
        if (parts.Length != 3 || parts[0] != "pbkdf2") return false;
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

public sealed record CheckoutAddressRequest(
    string RecipientName,
    string Phone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? StateProvince,
    string? PostalCode,
    string? CountryCode = "PK");

public sealed record CheckoutOrderLineRequest(Guid ProductId, int Quantity);

public sealed record CheckoutOrderRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string PaymentMethod,
    string? Notes,
    CheckoutAddressRequest ShippingAddress,
    IReadOnlyCollection<CheckoutOrderLineRequest> Lines,
    bool CreateAccount = false,
    string? Password = null);

public sealed record CheckoutPayRequest(bool Success = true, string? PaymentMethod = null);
public sealed record CustomerAuthRequest(string FirstName, string LastName, string Email, string Phone, string Password);
public sealed record CustomerLoginRequest(string Email, string Password);
