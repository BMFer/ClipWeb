using CLIPWEB.Application.Campaigns;
using CLIPWEB.Core.Entities;
using CLIPWEB.Infrastructure.Data;
using CLIPWEB.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CLIPWEB.Tests;

public class CampaignServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public CampaignServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        using var seed = CreateContext();
        seed.Database.EnsureCreated();
    }

    private ClipWebDbContext CreateContext()
        => new(new DbContextOptionsBuilder<ClipWebDbContext>().UseSqlite(_connection).Options);

    // Fresh service over a fresh context, mirroring the per-interaction scope.
    private CampaignService NewService(out ClipWebDbContext ctx)
    {
        ctx = CreateContext();
        return new CampaignService(
            new EfRepository<Brand>(ctx), new EfRepository<Campaign>(ctx));
    }

    private async Task<Guid> SeedBrandAsync(string name = "Acme")
    {
        var svc = NewService(out var ctx);
        using (ctx)
            return (await svc.CreateBrandAsync(name, null, null)).Id;
    }

    [Fact]
    public async Task CreateCampaign_links_brand_and_starts_active()
    {
        var brandId = await SeedBrandAsync();

        Guid campaignId;
        var svc = NewService(out var ctx);
        using (ctx)
        {
            var campaign = await svc.CreateCampaignAsync(
                brandId, "Launch Week", "Clip the keynote",
                new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null);
            campaignId = campaign.Id;
            Assert.True(campaign.IsActive);
            Assert.Equal(brandId, campaign.BrandId);
        }

        var verify = NewService(out var ctx2);
        using (ctx2)
        {
            var loaded = await verify.GetCampaignAsync(campaignId);
            Assert.NotNull(loaded);
            Assert.Equal("Launch Week", loaded!.Name);
        }
    }

    [Fact]
    public async Task CreateCampaign_throws_when_brand_is_unknown()
    {
        var svc = NewService(out var ctx);
        using (ctx)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.CreateCampaignAsync(Guid.NewGuid(), "Ghost", "x",
                    DateTime.UtcNow, null, null, null));
        }
    }

    [Fact]
    public async Task GetActiveCampaigns_excludes_closed_ones()
    {
        var brandId = await SeedBrandAsync();
        Guid toClose;

        var create = NewService(out var ctx);
        using (ctx)
        {
            await create.CreateCampaignAsync(brandId, "Open A", "x", DateTime.UtcNow, null, null, null);
            toClose = (await create.CreateCampaignAsync(brandId, "Closing B", "x", DateTime.UtcNow, null, null, null)).Id;
        }

        var close = NewService(out var ctx2);
        using (ctx2)
        {
            var closed = await close.CloseCampaignAsync(toClose);
            Assert.NotNull(closed);
            Assert.False(closed!.IsActive);
            Assert.NotNull(closed.EndDateUtc);
        }

        var query = NewService(out var ctx3);
        using (ctx3)
        {
            var active = await query.GetActiveCampaignsAsync();
            Assert.Single(active);
            Assert.Equal("Open A", active[0].Name);
        }
    }

    [Fact]
    public async Task CloseCampaign_returns_null_for_unknown_id()
    {
        var svc = NewService(out var ctx);
        using (ctx)
            Assert.Null(await svc.CloseCampaignAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task SearchCampaigns_filters_by_name_and_active_flag()
    {
        var brandId = await SeedBrandAsync();
        Guid closedId;

        var create = NewService(out var ctx);
        using (ctx)
        {
            await create.CreateCampaignAsync(brandId, "Crypto Clips", "x", DateTime.UtcNow, null, null, null);
            await create.CreateCampaignAsync(brandId, "Fitness Reels", "x", DateTime.UtcNow, null, null, null);
            closedId = (await create.CreateCampaignAsync(brandId, "Crypto Archive", "x", DateTime.UtcNow, null, null, null)).Id;
        }

        var close = NewService(out var ctx2);
        using (ctx2)
            await close.CloseCampaignAsync(closedId);

        var query = NewService(out var ctx3);
        using (ctx3)
        {
            var allCrypto = await query.SearchCampaignsAsync("crypto", activeOnly: false);
            Assert.Equal(2, allCrypto.Count);

            var activeCrypto = await query.SearchCampaignsAsync("crypto", activeOnly: true);
            Assert.Single(activeCrypto);
            Assert.Equal("Crypto Clips", activeCrypto[0].Name);
        }
    }

    public void Dispose() => _connection.Dispose();
}
