using Microsoft.EntityFrameworkCore;
using WristMatch.Application;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Tests;

public sealed class CatalogServiceTests
{
    [Fact]
    public async Task Budget_filter_excludes_products_above_limit()
    {
        await using var context = CreateContext();
        var service = new CatalogService(context);
        var result = await service.SearchAsync(new CatalogQuery(20000, null, null, null), CancellationToken.None);
        Assert.All(result.Items, product => Assert.True(product.Price <= 20000));
    }

    [Fact]
    public async Task Occasion_matching_is_ranked_first()
    {
        await using var context = CreateContext();
        var service = new CatalogService(context);
        var result = await service.SearchAsync(new CatalogQuery(null, Occasion.Wedding, null, null), CancellationToken.None);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, product => Assert.Contains("Wedding", product.Occasions));
    }

    private static WristMatchDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WristMatchDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var context = new WristMatchDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
