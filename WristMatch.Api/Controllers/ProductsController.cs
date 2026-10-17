using Microsoft.AspNetCore.Mvc;
using WristMatch.Application;
using WristMatch.Domain;

namespace WristMatch.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CatalogResult>> GetProducts(
        [FromQuery] decimal? maxBudget,
        [FromQuery] decimal? minBudget,
        [FromQuery] Occasion? occasion,
        [FromQuery] string? colour,
        [FromQuery] string? material,
        [FromQuery] string? search,
        [FromQuery] string sort = "recommended",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 50)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid pagination", detail: "Page must be positive and page size must be between 1 and 50.");
        return Ok(await catalogService.SearchAsync(new CatalogQuery(maxBudget, occasion, colour, material, search, minBudget, sort, page, pageSize), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductListItem>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var product = await catalogService.GetAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}
