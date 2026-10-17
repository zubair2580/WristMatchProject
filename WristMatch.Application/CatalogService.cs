using WristMatch.Domain;

namespace WristMatch.Application;

public sealed record CatalogQuery(decimal? MaxBudget, Occasion? Occasion, string? Colour, string? Material, string? Search = null, decimal? MinBudget = null, string Sort = "recommended", int Page = 1, int PageSize = 12);
public sealed record ProductListItem(Guid Id, string Slug, string Name, string Description, decimal Price, string Currency, string Colour, string Material, string ImageUrl, string RetailerUrl, decimal? CaseDiameterMm, IReadOnlyCollection<string> Occasions, int RecommendationScore, IReadOnlyCollection<string> Reasons);
public sealed record CatalogResult(IReadOnlyCollection<ProductListItem> Items, int Total, int Page, int PageSize);

public interface ICatalogService
{
    Task<CatalogResult> SearchAsync(CatalogQuery query, CancellationToken cancellationToken);
    Task<ProductListItem?> GetAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class CatalogService(IProductReadStore store) : ICatalogService
{
    public async Task<CatalogResult> SearchAsync(CatalogQuery query, CancellationToken cancellationToken)
    {
        var products = await store.QueryAsync(cancellationToken);
        var filtered = products.Where(product => product.IsActive);
        if (query.MaxBudget.HasValue)
            filtered = filtered.Where(product => product.Price <= query.MaxBudget.Value);
        if (query.MinBudget.HasValue)
            filtered = filtered.Where(product => product.Price >= query.MinBudget.Value);
        if (query.Occasion.HasValue)
            filtered = filtered.Where(product => product.Occasions.Any(item => item.Occasion == query.Occasion.Value));
        if (!string.IsNullOrWhiteSpace(query.Colour))
            filtered = filtered.Where(product => product.Colour.Equals(query.Colour, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(query.Material))
            filtered = filtered.Where(product => product.Material.Equals(query.Material, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            filtered = filtered.Where(product =>
                product.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                product.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                product.Colour.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                product.Material.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                product.Slug.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var ranked = filtered.Select(product => ToItem(product, query)).ToList();
        ranked = query.Sort.ToLowerInvariant() switch
        {
            "price-asc" => ranked.OrderBy(item => item.Price).ThenBy(item => item.Id).ToList(),
            "price-desc" => ranked.OrderByDescending(item => item.Price).ThenBy(item => item.Id).ToList(),
            "name" => ranked.OrderBy(item => item.Name).ThenBy(item => item.Id).ToList(),
            _ => ranked.OrderByDescending(item => item.RecommendationScore).ThenBy(item => item.Price).ThenBy(item => item.Id).ToList()
        };

        var total = ranked.Count;
        var items = ranked.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToArray();
        return new CatalogResult(items, total, query.Page, query.PageSize);
    }

    public async Task<ProductListItem?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = (await store.QueryAsync(cancellationToken)).SingleOrDefault(item => item.Id == id && item.IsActive);
        return product is null ? null : ToItem(product, new CatalogQuery(null, null, null, null));
    }

    private static ProductListItem ToItem(Product product, CatalogQuery query)
    {
        var reasons = new List<string>();
        var score = 0;
        if (query.Occasion.HasValue && product.Occasions.Any(item => item.Occasion == query.Occasion.Value))
        {
            score += 3;
            reasons.Add($"Matches {query.Occasion.Value}");
        }
        if (!string.IsNullOrWhiteSpace(query.Colour) && product.Colour.Equals(query.Colour, StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
            reasons.Add($"{product.Colour} colour");
        }
        if (!string.IsNullOrWhiteSpace(query.Material) && product.Material.Equals(query.Material, StringComparison.OrdinalIgnoreCase))
        {
            score++;
            reasons.Add($"{product.Material} material");
        }
        if (!string.IsNullOrWhiteSpace(query.Search) && product.Name.Contains(query.Search.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
            reasons.Add("Name match");
        }
        return new ProductListItem(product.Id, product.Slug, product.Name, product.Description, product.Price, product.Currency, product.Colour, product.Material, product.ImageUrl, product.RetailerUrl, product.CaseDiameterMm, product.Occasions.Select(item => item.Occasion.ToString()).ToArray(), score, reasons);
    }
}

public interface IProductReadStore
{
    Task<IReadOnlyCollection<Product>> QueryAsync(CancellationToken cancellationToken);
}
