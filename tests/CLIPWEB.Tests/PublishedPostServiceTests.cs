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

public class PublishedPostServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private int _clipSeq;

    public PublishedPostServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        using var seed = CreateContext();
        seed.Database.EnsureCreated();
    }

    private ClipWebDbContext CreateContext()
        => new(new DbContextOptionsBuilder<ClipWebDbContext>().UseSqlite(_connection).Options);

    private SubmissionService NewSubmissions(out ClipWebDbContext ctx)
    {
        ctx = CreateContext();
        var onboarding = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
        return new SubmissionService(
            new EfRepository<ClipSubmission>(ctx),
            new EfRepository<Campaign>(ctx),
            new EfRepository<EditorProfile>(ctx),
            onboarding);
    }

    private PublishedPostService NewPosts(out ClipWebDbContext ctx)
    {
        ctx = CreateContext();
        return new PublishedPostService(
            new EfRepository<PublishedPost>(ctx),
            new EfRepository<ClipSubmission>(ctx),
            new EfRepository<EditorProfile>(ctx));
    }

    private async Task<Guid> SeedCampaignAsync()
    {
        using var ctx = CreateContext();
        var campaigns = new CampaignService(new EfRepository<Brand>(ctx), new EfRepository<Campaign>(ctx));
        var brand = await campaigns.CreateBrandAsync("Acme", null, null);
        return (await campaigns.CreateCampaignAsync(brand.Id, "Launch", "Clip it", DateTime.UtcNow, null, null, null)).Id;
    }

    private async Task<Guid> SubmitAsync(ulong userId, Guid campaignId)
    {
        var svc = NewSubmissions(out var ctx);
        using (ctx)
            return (await svc.SubmitClipAsync(userId, $"user{userId}", campaignId, $"https://example.com/clip/{++_clipSeq}", null)).Id;
    }

    private async Task ApproveAsync(Guid submissionId)
    {
        var svc = NewSubmissions(out var ctx);
        using (ctx)
            await svc.SetStatusAsync(submissionId, SubmissionStatus.Approved);
    }

    [Fact]
    public async Task AddPost_to_approved_own_clip_persists_with_metrics()
    {
        var campaignId = await SeedCampaignAsync();
        var submissionId = await SubmitAsync(7UL, campaignId);
        await ApproveAsync(submissionId);

        var posts = NewPosts(out var ctx);
        using (ctx)
        {
            var post = await posts.AddPostAsync(
                7UL, submissionId, "TikTok", "https://tiktok.com/p/1", 1500, 200, 30, 10);
            Assert.Equal("TikTok", post.Platform);
            Assert.Equal(1500, post.Views);
        }

        using var verify = CreateContext();
        var saved = await verify.PublishedPosts.SingleAsync();
        Assert.Equal(submissionId, saved.ClipSubmissionId);
        Assert.Equal(200, saved.Likes);
        Assert.Equal(saved.PostedAtUtc, saved.LastUpdatedAtUtc);
    }

    [Fact]
    public async Task AddPost_to_pending_clip_throws()
    {
        var campaignId = await SeedCampaignAsync();
        var submissionId = await SubmitAsync(7UL, campaignId); // not approved

        var posts = NewPosts(out var ctx);
        using (ctx)
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                posts.AddPostAsync(7UL, submissionId, "TikTok", "https://tiktok.com/p/1", 100, null, null, null));
            Assert.Contains("approved", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task AddPost_to_another_editors_clip_throws()
    {
        var campaignId = await SeedCampaignAsync();
        var submissionId = await SubmitAsync(7UL, campaignId);
        await ApproveAsync(submissionId);

        // Editor 8 owns nothing here and tries to post to editor 7's clip.
        await SubmitAsync(8UL, campaignId); // gives editor 8 a profile

        var posts = NewPosts(out var ctx);
        using (ctx)
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                posts.AddPostAsync(8UL, submissionId, "TikTok", "https://tiktok.com/p/1", 100, null, null, null));
            Assert.Contains("your own", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task AddPost_rejects_duplicate_post_url()
    {
        var campaignId = await SeedCampaignAsync();
        var submissionId = await SubmitAsync(7UL, campaignId);
        await ApproveAsync(submissionId);

        var first = NewPosts(out var ctx1);
        using (ctx1)
            await first.AddPostAsync(7UL, submissionId, "TikTok", "https://tiktok.com/p/1", 100, null, null, null);

        var second = NewPosts(out var ctx2);
        using (ctx2)
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                second.AddPostAsync(7UL, submissionId, "TikTok", "https://TikTok.com/P/1", 200, null, null, null));
            Assert.Contains("already been logged", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task AddPost_to_unknown_submission_throws()
    {
        var posts = NewPosts(out var ctx);
        using (ctx)
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                posts.AddPostAsync(7UL, Guid.NewGuid(), "TikTok", "https://tiktok.com/p/1", 100, null, null, null));
    }

    [Fact]
    public async Task GetApprovedForEditor_returns_only_that_editors_approved_clips()
    {
        var campaignId = await SeedCampaignAsync();
        var approved = await SubmitAsync(7UL, campaignId);
        await ApproveAsync(approved);
        await SubmitAsync(7UL, campaignId);   // editor 7, still pending
        var otherApproved = await SubmitAsync(8UL, campaignId);
        await ApproveAsync(otherApproved);    // editor 8, approved

        var svc = NewSubmissions(out var ctx);
        using (ctx)
        {
            var list = await svc.GetApprovedForEditorAsync(7UL);
            var only = Assert.Single(list);
            Assert.Equal(approved, only.SubmissionId);
            Assert.Equal(SubmissionStatus.Approved, only.Status);
        }
    }

    public void Dispose() => _connection.Dispose();
}
