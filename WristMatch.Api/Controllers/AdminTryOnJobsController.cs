using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,OperationsManager,SupportViewer,CatalogManager")]
[ApiController]
[Route("api/v1/admin/tryon-jobs")]
public sealed class AdminTryOnJobsController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = from job in db.TryOnJobs.AsNoTracking()
                    join product in db.Products.AsNoTracking() on job.ProductId equals product.Id into products
                    from product in products.DefaultIfEmpty()
                    select new
                    {
                        job.Id,
                        Job = job.Id.ToString(),
                        Product = product != null ? product.Name : "Unknown product",
                        Created = job.CreatedAt,
                        State = job.State.ToString(),
                        ProviderTask = job.ProviderTaskId ?? "—",
                        Duration = job.CompletedAt.HasValue ? (int?)(job.CompletedAt.Value - job.CreatedAt).TotalSeconds : null,
                        job.ProviderMode,
                        job.SafeMessage
                    };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item => item.Product.ToLower().Contains(term) || item.State.ToLower().Contains(term) || item.Job.ToLower().Contains(term));
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(item => item.Created).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);
        return Ok(new { items, page, pageSize, total, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)) });
    }
}
