using CLIPWEB.Application.Campaigns;
using CLIPWEB.Application.Onboarding;
using CLIPWEB.Application.Submissions;
using CLIPWEB.Core.Entities;
using CLIPWEB.Core.Enums;
using CLIPWEB.Infrastructure.Data;
using CLIPWEB.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CLIPWEB.Tests;

public class SubmissionServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public SubmissionServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        using var seed = CreateContext();
        seed.Database.EnsureCreated();
    }

    private ClipWebDbContext CreateContext()
        => new(new DbContextOptionsBuilder<ClipWebDbContext>().UseSqlite(_connection).Options);

    private SubmissionService NewService(out ClipWebDbContext ctx)
    {
        ctx = CreateContext();
        var onboarding = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
        return new SubmissionService(
            new EfRepository<ClipSubmission>(ctx),
            new EfRepository<Campaign>(ctx),
            new EfRepository<EditorProfile>(ctx),
            onboarding);
    }

    private CampaignService NewCampaignService(out ClipWebDbContext ctx)
    {
        ctx = CreateContext();
        return new CampaignService(new EfRepository<Brand>(ctx), new EfRepository<Campaign>(ctx));
    }

    private async Task<Guid> SeedCampaignAsync(string name = "Launch Week", bool active = true)
    {
        Guid campaignId;
        using (var ctx = CreateContext())
        {
            var campaigns = new CampaignService(new EfRepository<Brand>(ctx), new EfRepository<Campaign>(ctx));
            var brand = await campaigns.CreateBrandAsync("Acme", null, null);
            campaignId = (await campaigns.CreateCampaignAsync(
                brand.Id, name, "Clip it", DateTime.UtcNow, null, null, null)).Id;
        }

        if (!active)
        {
            // A fresh context for the close, mirroring a separate interaction.
            var closer = NewCampaignService(out var closeCtx);
            using (closeCtx)
                await closer.CloseCampaignAsync(campaignId);
        }

        return campaignId;
    }

    [Fact]
    public async Task SubmitClip_creates_pending_submission_and_editor_profile()
    {
        var campaignId = await SeedCampaignAsync();

        var svc = NewService(out var ctx);
        using (ctx)
        {
            var submission = await svc.SubmitClipAsync(
                7UL, "clipper", campaignId, "https://example.com/clip", "first try");
            Assert.Equal(SubmissionStatus.Pending, submission.Status);
        }

        using var verify = CreateContext();
        Assert.Equal(1, await verify.ClipSubmissions.CountAsync());
        Assert.Equal(1, await verify.EditorProfiles.CountAsync(e => e.DiscordUserId == 7UL));
    }

    [Fact]
    public async Task SubmitClip_to_closed_campaign_throws()
    {
        var campaignId = await SeedCampaignAsync(active: false);

        var svc = NewService(out var ctx);
        using (ctx)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.SubmitClipAsync(7UL, "clipper", campaignId, "https://example.com/clip", null));
        }
    }

    [Fact]
    public async Task SubmitClip_to_unknown_campaign_throws()
    {
        var svc = NewService(out var ctx);
        using (ctx)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.SubmitClipAsync(7UL, "clipper", Guid.NewGuid(), "https://example.com/clip", null));
        }
    }

    [Fact]
    public async Task GetPending_summarizes_with_campaign_and_editor_names()
    {
        var campaignId = await SeedCampaignAsync("Crypto Clips");

        using (var ctx = CreateContext())
        {
            var onboarding = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
            await onboarding.SaveSurveyPart1Async(7UL, "clipper",
                new SurveyPart1("Jamie", null, null, null, null));
        }

        var submit = NewService(out var ctx1);
        using (ctx1)
            await submit.SubmitClipAsync(7UL, "clipper", campaignId, "https://example.com/clip", null);

        var query = NewService(out var ctx2);
        using (ctx2)
        {
            var pending = await query.GetPendingAsync();
            var only = Assert.Single(pending);
            Assert.Equal("Crypto Clips", only.CampaignName);
            Assert.Equal("Jamie", only.EditorName); // preferred name wins
            Assert.Equal(7UL, only.EditorDiscordUserId);
            Assert.Equal(SubmissionStatus.Pending, only.Status);
        }
    }

    [Fact]
    public async Task SetStatus_updates_and_removes_from_pending_queue()
    {
        var campaignId = await SeedCampaignAsync();

        Guid submissionId;
        var submit = NewService(out var ctx1);
        using (ctx1)
            submissionId = (await submit.SubmitClipAsync(7UL, "clipper", campaignId, "https://example.com/clip", null)).Id;

        var review = NewService(out var ctx2);
        using (ctx2)
        {
            var updated = await review.SetStatusAsync(submissionId, SubmissionStatus.Approved);
            Assert.NotNull(updated);
            Assert.Equal(SubmissionStatus.Approved, updated!.Status);
        }

        var query = NewService(out var ctx3);
        using (ctx3)
            Assert.Empty(await query.GetPendingAsync());
    }

    [Fact]
    public async Task SetStatus_returns_null_for_unknown_submission()
    {
        var svc = NewService(out var ctx);
        using (ctx)
            Assert.Null(await svc.SetStatusAsync(Guid.NewGuid(), SubmissionStatus.Approved));
    }

    public void Dispose() => _connection.Dispose();
}
