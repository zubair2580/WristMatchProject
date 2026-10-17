using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/v1/admin/menu-permissions")]
public sealed class AdminMenuPermissionsController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var items = await db.AdminMenuPermissions.AsNoTracking().OrderBy(item => item.RoleName).ThenBy(item => item.MenuKey).ToArrayAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(MenuPermissionRequest request, CancellationToken cancellationToken)
    {
        var item = await db.AdminMenuPermissions.SingleOrDefaultAsync(permission => permission.RoleName == request.RoleName && permission.MenuKey == request.MenuKey, cancellationToken);
        if (item is null)
        {
            item = new AdminMenuPermission(request.RoleName, request.MenuKey, request.CanView, request.CanCreate, request.CanUpdate, request.CanDelete);
            db.AdminMenuPermissions.Add(item);
        }
        else
            item.Update(request.CanView, request.CanCreate, request.CanUpdate, request.CanDelete);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(item);
    }
}

public sealed record MenuPermissionRequest(string RoleName, string MenuKey, bool CanView, bool CanCreate, bool CanUpdate, bool CanDelete);
