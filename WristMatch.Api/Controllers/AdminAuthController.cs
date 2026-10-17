using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
public sealed class AdminAuthController(SignInManager<AdminUser> signInManager, UserManager<AdminUser> userManager, WristMatchDbContext db) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return Unauthorized(new { code = "INVALID_CREDENTIALS", detail = "The email or password is not valid." });
        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);
        return result.Succeeded ? Ok(await BuildProfile(user)) : Unauthorized(new { code = "INVALID_CREDENTIALS", detail = "The email or password is not valid." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await userManager.GetUserAsync(User);
        return user is null ? Unauthorized() : Ok(await BuildProfile(user));
    }

    private async Task<object> BuildProfile(AdminUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var isSuperAdmin = roles.Contains("SuperAdmin");
        var permissions = await db.AdminMenuPermissions.AsNoTracking()
            .Where(item => roles.Contains(item.RoleName))
            .ToArrayAsync();
        var menus = permissions
            .GroupBy(item => item.MenuKey)
            .ToDictionary(
                group => group.Key,
                group => new MenuAccess(
                    isSuperAdmin || group.Any(item => item.CanView),
                    isSuperAdmin || group.Any(item => item.CanCreate),
                    isSuperAdmin || group.Any(item => item.CanUpdate),
                    isSuperAdmin || group.Any(item => item.CanDelete)));
        if (isSuperAdmin)
        {
            foreach (var key in new[] { "overview", "products", "categories", "customers", "orders", "invoices", "stock", "warehouses", "suppliers", "purchases", "accounts", "reports", "tryons", "users", "permissions" })
                menus[key] = new MenuAccess(true, true, true, true);
        }
        return new { id = user.Id, displayName = user.DisplayName, email = user.Email, roles, menus };
    }
}

public sealed record LoginRequest(string Email, string Password, bool RememberMe = false);
public sealed record MenuAccess(bool CanView, bool CanCreate, bool CanUpdate, bool CanDelete);
