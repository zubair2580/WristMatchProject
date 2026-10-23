using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WristMatch.Application;
using WristMatch.Domain;
using WristMatch.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
builder.Services.AddOpenApi();
builder.Services.AddDbContext<WristMatchDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
builder.Services.AddIdentity<AdminUser, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<WristMatchDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "wristmatch.admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/api/v1/admin/login";
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
});
builder.Services.AddScoped<IProductReadStore>(provider => provider.GetRequiredService<WristMatchDbContext>());
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IGuestSessionService, GuestSessionService>();
builder.Services.AddScoped<IPhotoAssetService, PhotoAssetService>();
builder.Services.AddScoped<ITryOnJobService, TryOnJobService>();
builder.Services.AddHttpClient<WristMatch.Infrastructure.YouCam.YouCamClient>();
builder.Services.AddScoped<ILookMatchService, WristMatch.Infrastructure.YouCam.LookMatchService>();
var youCamKey = builder.Configuration["YouCam:ApiKey"];
var preferYouCam = string.Equals(builder.Configuration["TryOn:ProviderMode"], "youcam", StringComparison.OrdinalIgnoreCase);
if (!string.IsNullOrWhiteSpace(youCamKey) && preferYouCam)
    builder.Services.AddScoped<ITryOnProvider, WristMatch.Infrastructure.YouCam.YouCamTryOnProvider>();
else
    builder.Services.AddScoped<ITryOnProvider, DemoTryOnProvider>();
builder.Services.AddHostedService<TryOnWorker>();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy.WithOrigins(builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:5173").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/api/v1/health/live", () => Results.Ok(new { status = "ok" }));

await using (var scope = app.Services.CreateAsyncScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    foreach (var role in new[] { "SuperAdmin", "CatalogManager", "OperationsManager", "PurchasingManager", "Accountant", "ReportViewer", "OperationsViewer", "SupportViewer" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));

    var bootstrapEmail = builder.Configuration["Admin:BootstrapEmail"];
    var bootstrapPassword = builder.Configuration["Admin:BootstrapPassword"];
    if (!string.IsNullOrWhiteSpace(bootstrapEmail) && !string.IsNullOrWhiteSpace(bootstrapPassword))
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
        var bootstrapUser = await userManager.FindByEmailAsync(bootstrapEmail);
        if (bootstrapUser is null)
        {
            var user = new AdminUser { UserName = bootstrapEmail, Email = bootstrapEmail, DisplayName = builder.Configuration["Admin:BootstrapDisplayName"] ?? "WristMatch Administrator", EmailConfirmed = true, IsActive = true };
            var result = await userManager.CreateAsync(user, bootstrapPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, "SuperAdmin");
        }
        else
        {
            if (!await userManager.CheckPasswordAsync(bootstrapUser, bootstrapPassword))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(bootstrapUser);
                await userManager.ResetPasswordAsync(bootstrapUser, token, bootstrapPassword);
            }
            if (!bootstrapUser.IsActive)
            {
                bootstrapUser.IsActive = true;
                await userManager.UpdateAsync(bootstrapUser);
            }
            if (!await userManager.IsInRoleAsync(bootstrapUser, "SuperAdmin"))
                await userManager.AddToRoleAsync(bootstrapUser, "SuperAdmin");
            if (await userManager.IsLockedOutAsync(bootstrapUser))
                await userManager.SetLockoutEndDateAsync(bootstrapUser, null);
        }
    }

    if (builder.Configuration.GetValue("Admin:SeedDemoUsers", false))
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
        var demoUsers = new[]
        {
            ("catalog.manager@wristmatch.local", "Demo Catalog Manager", "CatalogManager"),
            ("operations.manager@wristmatch.local", "Demo Operations Manager", "OperationsManager"),
            ("purchasing.manager@wristmatch.local", "Demo Purchasing Manager", "PurchasingManager"),
            ("accountant@wristmatch.local", "Demo Accountant", "Accountant"),
            ("report.viewer@wristmatch.local", "Demo Report Viewer", "ReportViewer")
        };
        foreach (var demo in demoUsers)
        {
            var existing = await userManager.FindByEmailAsync(demo.Item1);
            if (existing is not null)
            {
                if (!await userManager.IsInRoleAsync(existing, demo.Item3))
                    await userManager.AddToRoleAsync(existing, demo.Item3);
                continue;
            }
            var user = new AdminUser { UserName = demo.Item1, Email = demo.Item1, DisplayName = demo.Item2, EmailConfirmed = true, IsActive = true };
            var result = await userManager.CreateAsync(user, "Admin@WristMatch2026!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, demo.Item3);
        }
    }

    var permissionDb = scope.ServiceProvider.GetRequiredService<WristMatchDbContext>();
    if (!await permissionDb.AdminMenuPermissions.AnyAsync())
    {
        var roleNames = new[] { "SuperAdmin", "CatalogManager", "OperationsManager", "PurchasingManager", "Accountant", "ReportViewer", "SupportViewer" };
        var menuKeys = new[] { "overview", "products", "categories", "customers", "orders", "invoices", "stock", "warehouses", "suppliers", "purchases", "accounts", "reports", "tryons", "users", "permissions" };
        foreach (var role in roleNames)
            foreach (var menu in menuKeys)
            {
                var canCreate = role == "SuperAdmin" || role == "CatalogManager" && (menu == "products" || menu == "categories");
                var canUpdate = role != "ReportViewer" && role != "SupportViewer";
                permissionDb.AdminMenuPermissions.Add(new AdminMenuPermission(role, menu, role == "SuperAdmin" || menu is "overview" or "products" or "categories" or "customers", canCreate, canUpdate, role == "SuperAdmin"));
            }
        await permissionDb.SaveChangesAsync();
    }

    var stockDb = scope.ServiceProvider.GetRequiredService<WristMatchDbContext>();
    var primaryWarehouse = await stockDb.Warehouses.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.Code).Select(item => item.Id).FirstOrDefaultAsync();
    if (primaryWarehouse != Guid.Empty)
    {
        var productIds = await stockDb.Products.AsNoTracking().Where(item => item.IsActive).Select(item => item.Id).ToListAsync();
        var existing = await stockDb.InventoryBalances.AsNoTracking()
            .Where(item => item.WarehouseId == primaryWarehouse)
            .Select(item => item.ProductId)
            .ToListAsync();
        var missing = productIds.Except(existing).ToList();
        if (missing.Count > 0)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var productId in missing)
            {
                var balance = new InventoryBalance(primaryWarehouse, productId);
                balance.ApplyReceipt(40, 1200m, now);
                stockDb.InventoryBalances.Add(balance);
                stockDb.StockMovements.Add(new StockMovement(primaryWarehouse, productId, "AdjustmentIn", 40, 1200m, $"SEED-{now:yyyyMMdd}", "Bootstrap stock for primary warehouse"));
            }
            await stockDb.SaveChangesAsync();
        }
    }
}

app.Run();

public partial class Program { }
