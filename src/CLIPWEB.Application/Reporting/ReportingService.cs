using CLIPWEB.Core.Entities;
using CLIPWEB.Core.Enums;
using CLIPWEB.Core.Interfaces;

namespace CLIPWEB.Application.Reporting;

/// <inheritdoc />
public class ReportingService : IReportingService
{
    private readonly IRepository<ClipSubmission> _submissions;
    private readonly IRepository<PublishedPost> _posts;
    private readonly IRepository<EditorProfile> _editors;
    private readonly IRepository<Campaign> _campaigns;
    private readonly IRepository<Brand> _brands;

    public ReportingService(
        IRepository<ClipSubmission> submissions,
        IRepository<PublishedPost> posts,
        IRepository<EditorProfile> editors,
        IRepository<Campaign> campaigns,
        IRepository<Brand> brands)
    {
        _submissions = submissions;
        _posts = posts;
        _editors = editors;
        _campaigns = campaigns;
        _brands = brands;
    }

    public async Task<EditorStats?> GetEditorStatsByDiscordIdAsync(ulong discordUserId, CancellationToken ct = default)
    {
        var editor = (await _editors.ListAsync(e => e.DiscordUserId == discordUserId, ct)).FirstOrDefault();
        return editor is null ? null : await BuildEditorStatsAsync(editor, ct);
    }

    public async Task<EditorStats?> GetEditorStatsAsync(Guid editorProfileId, CancellationToken ct = default)
    {
        var editor = (await _editors.ListAsync(e => e.Id == editorProfileId, ct)).FirstOrDefault();
        return editor is null ? null : await BuildEditorStatsAsync(editor, ct);
    }

    private async Task<EditorStats> BuildEditorStatsAsync(EditorProfile editor, CancellationToken ct)
    {
        var subs = await _submissions.ListAsync(s => s.EditorProfileId == editor.Id, ct);
        var posts = await PostsForSubmissionsAsync(subs, ct);

        var totalSubmissions = subs.Count;
        var approved = subs.Count(s => s.Status == SubmissionStatus.Approved);
        var totalViews = posts.Sum(p => p.Views);
        var best = posts.OrderByDescending(p => p.Views).FirstOrDefault();

        var campaignIds = subs.Select(s => s.CampaignId).Distinct().ToList();
        var campaigns = campaignIds.Count == 0
            ? []
            : await _campaigns.ListAsync(c => campaignIds.Contains(c.Id), ct);

        return new EditorStats(
            editor.PreferredName ?? editor.DiscordUsername,
            editor.DiscordUserId,
            totalSubmissions,
            approved,
            totalSubmissions == 0 ? 0 : (double)approved / totalSubmissions,
            posts.Count,
            totalViews,
            Average(totalViews, posts.Count),
            best?.PostUrl,
            best?.Views,
            campaigns.Count(c => c.IsActive));
    }

    public async Task<CampaignReport?> GetCampaignReportAsync(Guid campaignId, CancellationToken ct = default)
    {
        var campaign = (await _campaigns.ListAsync(c => c.Id == campaignId, ct)).FirstOrDefault();
        if (campaign is null)
            return null;

        var brand = (await _brands.ListAsync(b => b.Id == campaign.BrandId, ct)).FirstOrDefault();
        var subs = await _submissions.ListAsync(s => s.CampaignId == campaignId, ct);
        var posts = await PostsForSubmissionsAsync(subs, ct);

        var totalViews = posts.Sum(p => p.Views);
        var topPost = posts.OrderByDescending(p => p.Views).FirstOrDefault();

        // Top editor by total views across their posts in this campaign.
        var submissionToEditor = subs.ToDictionary(s => s.Id, s => s.EditorProfileId);
        var topEditorGroup = posts
            .GroupBy(p => submissionToEditor[p.ClipSubmissionId])
            .Select(g => new { EditorId = g.Key, Views = g.Sum(p => p.Views) })
            .OrderByDescending(x => x.Views)
            .FirstOrDefault();

        string? topEditorName = null;
        if (topEditorGroup is not null)
        {
            var topEditor = (await _editors.ListAsync(e => e.Id == topEditorGroup.EditorId, ct)).FirstOrDefault();
            topEditorName = topEditor?.PreferredName ?? topEditor?.DiscordUsername;
        }

        return new CampaignReport(
            campaign.Name,
            brand?.Name ?? "Unknown",
            campaign.IsActive,
            subs.Count,
            subs.Count(s => s.Status == SubmissionStatus.Approved),
            posts.Count,
            totalViews,
            Average(totalViews, posts.Count),
            topEditorName,
            topEditorGroup?.Views,
            topPost?.PostUrl,
            topPost?.Views,
            subs.Select(s => s.EditorProfileId).Distinct().Count());
    }

    public async Task<BrandSummary?> GetBrandSummaryAsync(Guid brandId, CancellationToken ct = default)
    {
        var brand = (await _brands.ListAsync(b => b.Id == brandId, ct)).FirstOrDefault();
        if (brand is null)
            return null;

        var campaigns = await _campaigns.ListAsync(c => c.BrandId == brandId, ct);
        var campaignIds = campaigns.Select(c => c.Id).ToList();
        var subs = campaignIds.Count == 0
            ? []
            : await _submissions.ListAsync(s => campaignIds.Contains(s.CampaignId), ct);
        var posts = await PostsForSubmissionsAsync(subs, ct);

        var totalViews = posts.Sum(p => p.Views);

        // Top campaign by total views.
        var submissionToCampaign = subs.ToDictionary(s => s.Id, s => s.CampaignId);
        var topCampaignGroup = posts
            .GroupBy(p => submissionToCampaign[p.ClipSubmissionId])
            .Select(g => new { CampaignId = g.Key, Views = g.Sum(p => p.Views) })
            .OrderByDescending(x => x.Views)
            .FirstOrDefault();
        var topCampaignName = topCampaignGroup is null
            ? null
            : campaigns.FirstOrDefault(c => c.Id == topCampaignGroup.CampaignId)?.Name;

        return new BrandSummary(
            brand.Name,
            campaigns.Count,
            campaigns.Count(c => c.IsActive),
            subs.Count,
            subs.Count(s => s.Status == SubmissionStatus.Approved),
            posts.Count,
            totalViews,
            Average(totalViews, posts.Count),
            topCampaignName,
            topCampaignGroup?.Views);
    }

    public async Task<IReadOnlyList<EditorOption>> SearchEditorsAsync(
        string? query, int take = 25, CancellationToken ct = default)
    {
        IReadOnlyList<EditorProfile> editors;
        if (string.IsNullOrWhiteSpace(query))
        {
            editors = await _editors.ListAsync(ct: ct);
        }
        else
        {
            var q = query.ToLower();
            editors = await _editors.ListAsync(
                e => e.DiscordUsername.ToLower().Contains(q)
                  || (e.PreferredName != null && e.PreferredName.ToLower().Contains(q)), ct);
        }

        return editors
            .Select(e => new EditorOption(e.Id, e.PreferredName ?? e.DiscordUsername))
            .OrderBy(e => e.Name)
            .Take(take)
            .ToList();
    }

    public async Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(int take = 10, CancellationToken ct = default)
    {
        var posts = await _posts.ListAsync(ct: ct);
        if (posts.Count == 0)
            return [];

        var submissionIds = posts.Select(p => p.ClipSubmissionId).Distinct().ToList();
        var subs = await _submissions.ListAsync(s => submissionIds.Contains(s.Id), ct);
        var submissionToEditor = subs.ToDictionary(s => s.Id, s => s.EditorProfileId);

        var ranked = posts
            .Where(p => submissionToEditor.ContainsKey(p.ClipSubmissionId))
            .GroupBy(p => submissionToEditor[p.ClipSubmissionId])
            .Select(g => new { EditorId = g.Key, Views = g.Sum(p => p.Views), Posts = g.Count() })
            .OrderByDescending(x => x.Views)
            .ThenByDescending(x => x.Posts)
            .Take(take)
            .ToList();

        var editorIds = ranked.Select(x => x.EditorId).ToList();
        var editors = (await _editors.ListAsync(e => editorIds.Contains(e.Id), ct))
            .ToDictionary(e => e.Id);

        return ranked
            .Select((x, i) =>
            {
                editors.TryGetValue(x.EditorId, out var editor);
                var name = editor?.PreferredName ?? editor?.DiscordUsername ?? "Unknown editor";
                return new LeaderboardEntry(i + 1, name, x.Views, x.Posts);
            })
            .ToList();
    }

    private async Task<IReadOnlyList<PublishedPost>> PostsForSubmissionsAsync(
        IReadOnlyList<ClipSubmission> submissions, CancellationToken ct)
    {
        if (submissions.Count == 0)
            return [];

        var ids = submissions.Select(s => s.Id).ToList();
        return await _posts.ListAsync(p => ids.Contains(p.ClipSubmissionId), ct);
    }

    private static double Average(long total, int count)
        => count == 0 ? 0 : (double)total / count;
}
