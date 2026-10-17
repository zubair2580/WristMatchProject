using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/v1/admin/users")]
public sealed class AdminUsersController(UserManager<AdminUser> users, RoleManager<IdentityRole<Guid>> roles) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = users.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(user => user.DisplayName.Contains(search) || user.Email!.Contains(search));
        var total = await query.CountAsync();
        var pageUsers = await query.OrderBy(user => user.DisplayName).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync();
        var summaries = new List<AdminUserSummary>();
        foreach (var user in pageUsers)
            summaries.Add(new AdminUserSummary(user.Id, user.DisplayName, user.Email ?? string.Empty, user.IsActive, user.CreatedAt, (await users.GetRolesAsync(user)).ToArray()));
        return Ok(new { items = summaries, page, pageSize, total, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)), roles = await roles.Roles.OrderBy(role => role.Name).Select(role => role.Name!).ToArrayAsync() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AdminUserSummary>> Create(CreateAdminUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { code = "INVALID_USER", detail = "Display name, email, and password are required." });
        if (!await roles.RoleExistsAsync(request.Role))
            return BadRequest(new { code = "INVALID_ROLE", detail = "The selected role does not exist." });
        var user = new AdminUser { UserName = request.Email.Trim(), Email = request.Email.Trim(), DisplayName = request.DisplayName.Trim(), EmailConfirmed = true, IsActive = true };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return ValidationProblem(new ValidationProblemDetails(result.Errors.GroupBy(error => error.Code).ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray())));
        await users.AddToRoleAsync(user, request.Role);
        return Created($"/api/v1/admin/users/{user.Id}", await ToSummary(user));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, UpdateAdminUserRequest request)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        if (!await roles.RoleExistsAsync(request.Role)) return BadRequest(new { code = "INVALID_ROLE", detail = "The selected role does not exist." });
        user.DisplayName = request.DisplayName.Trim();
        user.Email = request.Email.Trim();
        user.UserName = request.Email.Trim();
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded) return ValidationProblem(new ValidationProblemDetails(result.Errors.GroupBy(error => error.Code).ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray())));
        await users.RemoveFromRolesAsync(user, await users.GetRolesAsync(user));
        await users.AddToRoleAsync(user, request.Role);
        return Ok(await ToSummary(user));
    }

    [HttpPost("{id:guid}/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordRequest request)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Password)) return BadRequest(new { code = "PASSWORD_REQUIRED", detail = "A new password is required." });
        var token = await users.GeneratePasswordResetTokenAsync(user);
        var result = await users.ResetPasswordAsync(user, token, request.Password);
        return result.Succeeded ? NoContent() : ValidationProblem(new ValidationProblemDetails(result.Errors.GroupBy(error => error.Code).ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray())));
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        user.IsActive = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await users.UpdateAsync(user);
        return NoContent();
    }

    private async Task<AdminUserSummary> ToSummary(AdminUser user) => new(user.Id, user.DisplayName, user.Email ?? string.Empty, user.IsActive, user.CreatedAt, (await users.GetRolesAsync(user)).ToArray());
}

public sealed record AdminUserSummary(Guid Id, string DisplayName, string Email, bool IsActive, DateTimeOffset CreatedAt, IReadOnlyCollection<string> Roles);
public sealed record CreateAdminUserRequest(string DisplayName, string Email, string Password, string Role);
public sealed record UpdateAdminUserRequest(string DisplayName, string Email, string Role, bool IsActive);
public sealed record ResetPasswordRequest(string Password);
