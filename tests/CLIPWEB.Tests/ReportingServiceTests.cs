using CLIPWEB.Application.Campaigns;
using CLIPWEB.Application.Onboarding;
using CLIPWEB.Application.Reporting;
using CLIPWEB.Application.Submissions;
using CLIPWEB.Core.Entities;
using CLIPWEB.Core.Enums;
using CLIPWEB.Infrastructure.Data;
using CLIPWEB.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CLIPWEB.Tests;

public class ReportingServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private Guid _brandId;
    private Guid _campaignId;
    private int _clipSeq;

    public ReportingServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        using var seed = CreateContext();
        seed.Database.EnsureCreated();
    }

    private ClipWebDbContext CreateContext()
        => new(new DbContextOptionsBuilder<ClipWebDbContext>().UseSqlite(_connection).Options);

    private ReportingService NewReporting(out ClipWebDbContext ctx)
    {
        ctx = CreateContext();
        return new ReportingService(
            new EfRepository<ClipSubmission>(ctx),
            new EfRepository<PublishedPost>(ctx),
            new EfRepository<EditorProfile>(ctx),
            new EfRepository<Campaign>(ctx),
            new EfRepository<Brand>(ctx));
    }

    private async Task SeedBrandAndCampaignAsync()
    {
        using var ctx = CreateContext();
        var campaigns = new CampaignService(new EfRepository<Brand>(ctx), new EfRepository<Campaign>(ctx));
        var brand = await campaigns.CreateBrandAsync("Acme", null, null);
        _brandId = brand.Id;
        _campaignId = (await campaigns.CreateCampaignAsync(brand.Id, "Launch", "Clip it", DateTime.UtcNow, null, null, null)).Id;
    }

    private async Task SetPreferredNameAsync(ulong userId, string name)
    {
        using var ctx = CreateContext();
        var onboarding = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
        await onboarding.SaveSurveyPart1Async(userId, $"user{userId}", new SurveyPart1(name, null, null, null, null));
    }

    private async Task<Guid> SubmitAsync(ulong userId)
    {
        using var ctx = CreateContext();
        var onboarding = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
        var submissions = new SubmissionService(
            new EfRepository<ClipSubmission>(ctx), new EfRepository<Campaign>(ctx),
            new EfRepository<EditorProfile>(ctx), onboarding);
        return (await submissions.SubmitClipAsync(userId, $"user{userId}", _campaignId, $"https://example.com/clip/{++_clipSeq}", null)).Id;
    }

    private async Task ApproveAsync(Guid submissionId)
    {
        using var ctx = CreateContext();
        var onboarding = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
        var submissions = new SubmissionService(
            new EfRepository<ClipSubmission>(ctx), new EfRepository<Campaign>(ctx),
            new EfRepository<EditorProfile>(ctx), onboarding);
        await submissions.SetStatusAsync(submissionId, SubmissionStatus.Approved);
    }

    private async Task AddPostAsync(ulong userId, Guid submissionId, long views)
    {
        using var ctx = CreateContext();
        var posts = new PublishedPostService(
            new EfRepository<PublishedPost>(ctx), new EfRepository<ClipSubmission>(ctx),
            new EfRepository<EditorProfile>(ctx));
        await posts.AddPostAsync(userId, submissionId, "TikTok", $"https://tiktok.com/{views}", views, null, null, null);
    }

    /// <summary>
    /// Scenario: editor 7 ("Jamie") submits 2, one approved with a 1000-view post;
    /// editor 8 submits 1, approved with a 500-view post.
    /// </summary>
    private async Task SeedScenarioAsync()
    {
        await SeedBrandAndCampaignAsync();
        await SetPreferredNameAsync(7UL, "Jamie");

        var s7a = await SubmitAsync(7UL);
        await SubmitAsync(7UL);             // editor 7, stays pending
        await ApproveAsync(s7a);
        await AddPostAsync(7UL, s7a, 1000);

        var s8 = await SubmitAsync(8UL);
        await ApproveAsync(s8);
        await AddPostAsync(8UL, s8, 500);
    }

    [Fact]
    public async Task EditorStats_aggregates_submissions_posts_and_views()
    {
        await SeedScenarioAsync();

        var reporting = NewReporting(out var ctx);
        using (ctx)
        {
            var stats = await reporting.GetEditorStatsByDiscordIdAsync(7UL);
            Assert.NotNull(stats);
            Assert.Multiple(
                () => Assert.Equal("Jamie", stats!.EditorName),
                () => Assert.Equal(2, stats!.TotalSubmissions),
                () => Assert.Equal(1, stats!.ApprovedSubmissions),
                () => Assert.Equal(0.5, stats!.ApprovalRate),
                () => Assert.Equal(1, stats!.TotalPosts),
                () => Assert.Equal(1000, stats!.TotalViews),
                () => Assert.Equal(1000, stats!.AverageViewsPerPost),
                () => Assert.Equal(1000, stats!.BestPostViews),
                () => Assert.Equal(1, stats!.ActiveCampaignsWorked));
        }
    }

    [Fact]
    public async Task CampaignReport_totals_and_leaders_are_correct()
    {
        await SeedScenarioAsync();

        var reporting = NewReporting(out var ctx);
        using (ctx)
        {
            var report = await reporting.GetCampaignReportAsync(_campaignId);
            Assert.NotNull(report);
            Assert.Multiple(
                () => Assert.Equal("Launch", report!.CampaignName),
                () => Assert.Equal("Acme", report!.BrandName),
                () => Assert.Equal(3, report!.TotalClipsSubmitted),
                () => Assert.Equal(2, report!.TotalClipsApproved),
                () => Assert.Equal(2, report!.TotalPostsPublished),
                () => Assert.Equal(1500, report!.TotalViews),
                () => Assert.Equal(750, report!.AverageViewsPerPost),
                () => Assert.Equal("Jamie", report!.TopEditorName),
                () => Assert.Equal(1000, report!.TopEditorViews),
                () => Assert.Equal(1000, report!.TopPostViews),
                () => Assert.Equal(2, report!.ActiveEditors));
        }
    }

    [Fact]
    public async Task BrandSummary_rolls_up_across_campaigns()
    {
        await SeedScenarioAsync();

        var reporting = NewReporting(out var ctx);
        using (ctx)
        {
            var summary = await reporting.GetBrandSummaryAsync(_brandId);
            Assert.NotNull(summary);
            Assert.Multiple(
                () => Assert.Equal("Acme", summary!.BrandName),
                () => Assert.Equal(1, summary!.CampaignCount),
                () => Assert.Equal(1, summary!.ActiveCampaignCount),
                () => Assert.Equal(3, summary!.TotalClipsSubmitted),
                () => Assert.Equal(1500, summary!.TotalViews),
                () => Assert.Equal("Launch", summary!.TopCampaignName),
                () => Assert.Equal(1500, summary!.TopCampaignViews));
        }
    }

    [Fact]
    public async Task EditorStats_is_null_for_unknown_user()
    {
        await SeedBrandAndCampaignAsync();
        var reporting = NewReporting(out var ctx);
        using (ctx)
            Assert.Null(await reporting.GetEditorStatsByDiscordIdAsync(404UL));
    }

    [Fact]
    public async Task Leaderboard_ranks_editors_by_total_views()
    {
        await SeedScenarioAsync();

        var reporting = NewReporting(out var ctx);
        using (ctx)
        {
            var board = await reporting.GetLeaderboardAsync();
            Assert.Equal(2, board.Count);
            Assert.Equal(1, board[0].Rank);
            Assert.Equal("Jamie", board[0].EditorName);   // 1000 views beats 500
            Assert.Equal(1000, board[0].TotalViews);
            Assert.Equal(2, board[1].Rank);
            Assert.Equal(500, board[1].TotalViews);
        }
    }

    [Fact]
    public async Task SearchEditors_matches_by_preferred_name()
    {
        await SeedScenarioAsync();
        var reporting = NewReporting(out var ctx);
        using (ctx)
        {
            var matches = await reporting.SearchEditorsAsync("jam");
            var only = Assert.Single(matches);
            Assert.Equal("Jamie", only.Name);
        }
    }

    public void Dispose() => _connection.Dispose();
}
