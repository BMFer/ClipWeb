using CLIPWEB.Application.Reporting;
using Discord;

namespace CLIPWEB.Application.Commands;

/// <summary>Embed builders for the reporting commands.</summary>
internal static class ReportEmbeds
{
    private static readonly Color Accent = new(0x5865F2);

    public static Embed Editor(EditorStats s, string title) => new EmbedBuilder()
        .WithTitle(title)
        .WithColor(Accent)
        .AddField("Clips submitted", s.TotalSubmissions.ToString("N0"), inline: true)
        .AddField("Clips approved", s.ApprovedSubmissions.ToString("N0"), inline: true)
        .AddField("Approval rate", Percent(s.ApprovalRate), inline: true)
        .AddField("Posts published", s.TotalPosts.ToString("N0"), inline: true)
        .AddField("Total views", s.TotalViews.ToString("N0"), inline: true)
        .AddField("Avg views/post", s.AverageViewsPerPost.ToString("N0"), inline: true)
        .AddField("Active campaigns", s.ActiveCampaignsWorked.ToString("N0"), inline: true)
        .AddField("Best post", BestPost(s.BestPostUrl, s.BestPostViews))
        .Build();

    public static Embed Campaign(CampaignReport r) => new EmbedBuilder()
        .WithTitle($"Campaign report · {r.CampaignName}")
        .WithColor(r.IsActive ? Color.Green : Color.LightGrey)
        .WithDescription($"Brand: **{r.BrandName}** · {(r.IsActive ? "🟢 Active" : "🔒 Closed")}")
        .AddField("Clips submitted", r.TotalClipsSubmitted.ToString("N0"), inline: true)
        .AddField("Clips approved", r.TotalClipsApproved.ToString("N0"), inline: true)
        .AddField("Posts published", r.TotalPostsPublished.ToString("N0"), inline: true)
        .AddField("Total views", r.TotalViews.ToString("N0"), inline: true)
        .AddField("Avg views/post", r.AverageViewsPerPost.ToString("N0"), inline: true)
        .AddField("Active editors", r.ActiveEditors.ToString("N0"), inline: true)
        .AddField("Top editor", Leader(r.TopEditorName, r.TopEditorViews), inline: true)
        .AddField("Top post", BestPost(r.TopPostUrl, r.TopPostViews))
        .Build();

    public static Embed Brand(BrandSummary b) => new EmbedBuilder()
        .WithTitle($"Brand summary · {b.BrandName}")
        .WithColor(Accent)
        .WithDescription($"{b.CampaignCount} campaign(s) · {b.ActiveCampaignCount} active")
        .AddField("Clips submitted", b.TotalClipsSubmitted.ToString("N0"), inline: true)
        .AddField("Clips approved", b.TotalClipsApproved.ToString("N0"), inline: true)
        .AddField("Posts published", b.TotalPostsPublished.ToString("N0"), inline: true)
        .AddField("Total views", b.TotalViews.ToString("N0"), inline: true)
        .AddField("Avg views/post", b.AverageViewsPerPost.ToString("N0"), inline: true)
        .AddField("Top campaign", Leader(b.TopCampaignName, b.TopCampaignViews), inline: true)
        .Build();

    public static Embed Leaderboard(IReadOnlyList<LeaderboardEntry> entries)
    {
        var lines = entries.Select(e =>
            $"{Medal(e.Rank)} **{e.Rank}.** {e.EditorName} — {e.TotalViews:N0} views ({e.Posts:N0} posts)");

        return new EmbedBuilder()
            .WithTitle("🏆 Editor leaderboard")
            .WithColor(Accent)
            .WithDescription(string.Join('\n', lines))
            .WithFooter("Ranked by total views generated.")
            .Build();
    }

    private static string Medal(int rank) => rank switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => "▫️"
    };

    private static string Percent(double rate) => $"{rate * 100:N0}%";

    private static string BestPost(string? url, long? views)
        => url is null ? "—" : $"{url} ({views:N0} views)";

    private static string Leader(string? name, long? views)
        => name is null ? "—" : $"{name} ({views:N0} views)";
}
