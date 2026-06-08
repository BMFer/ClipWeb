namespace CLIPWEB.Application.Reporting;

/// <summary>Aggregated performance for a single editor.</summary>
public record EditorStats(
    string EditorName,
    ulong DiscordUserId,
    int TotalSubmissions,
    int ApprovedSubmissions,
    double ApprovalRate,
    int TotalPosts,
    long TotalViews,
    double AverageViewsPerPost,
    string? BestPostUrl,
    long? BestPostViews,
    int ActiveCampaignsWorked);

/// <summary>Aggregated totals for a single campaign.</summary>
public record CampaignReport(
    string CampaignName,
    string BrandName,
    bool IsActive,
    int TotalClipsSubmitted,
    int TotalClipsApproved,
    int TotalPostsPublished,
    long TotalViews,
    double AverageViewsPerPost,
    string? TopEditorName,
    long? TopEditorViews,
    string? TopPostUrl,
    long? TopPostViews,
    int ActiveEditors);

/// <summary>Aggregated performance across all of a brand's campaigns.</summary>
public record BrandSummary(
    string BrandName,
    int CampaignCount,
    int ActiveCampaignCount,
    int TotalClipsSubmitted,
    int TotalClipsApproved,
    int TotalPostsPublished,
    long TotalViews,
    double AverageViewsPerPost,
    string? TopCampaignName,
    long? TopCampaignViews);

/// <summary>An editor choice for autocomplete (value = profile id).</summary>
public record EditorOption(Guid EditorProfileId, string Name);

/// <summary>A ranked entry in the editor leaderboard.</summary>
public record LeaderboardEntry(int Rank, string EditorName, long TotalViews, int Posts);
