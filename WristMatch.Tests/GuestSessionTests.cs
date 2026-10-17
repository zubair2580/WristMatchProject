using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WristMatch.Infrastructure;

namespace WristMatch.Tests;

public sealed class GuestSessionTests
{
    [Fact]
    public async Task Session_token_is_reused_only_for_the_matching_owner()
    {
        await using var context = new WristMatchDbContext(new DbContextOptionsBuilder<WristMatchDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Privacy:SessionHours"] = "24",
            ["Privacy:SessionTokenLength"] = "32"
        }).Build();
        var service = new GuestSessionService(context, configuration);

        var first = await service.GetOrCreateAsync(null, CancellationToken.None);
        var reused = await service.GetOrCreateAsync(first.RawToken, CancellationToken.None);
        var unknown = await service.FindAsync("not-the-session-token", CancellationToken.None);

        Assert.True(first.Created);
        Assert.False(reused.Created);
        Assert.Equal(first.Session.Id, reused.Session.Id);
        Assert.NotEqual(first.RawToken, first.Session.TokenHash);
        Assert.Null(unknown);
    }
}
