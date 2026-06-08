namespace CLIPWEB.Application.Reporting;

/// <summary>
/// Read-only aggregation across submissions, posts, editors, campaigns and
/// brands for the Phase 6 reporting commands.
/// </summary>
public interface IReportingService
{
    /// <summary>Stats for the editor with the given Discord id, or null if they have no profile.</summary>
    Task<EditorStats?> GetEditorStatsByDiscordIdAsync(ulong discordUserId, CancellationToken ct = default);

    /// <summary>Stats for a specific editor profile, or null if it doesn't exist.</summary>
    Task<EditorStats?> GetEditorStatsAsync(Guid editorProfileId, CancellationToken ct = default);

    /// <summary>Totals for a campaign, or null if it doesn't exist.</summary>
    Task<CampaignReport?> GetCampaignReportAsync(Guid campaignId, CancellationToken ct = default);

    /// <summary>Performance across a brand's campaigns, or null if the brand doesn't exist.</summary>
    Task<BrandSummary?> GetBrandSummaryAsync(Guid brandId, CancellationToken ct = default);

    /// <summary>Editors matching a query against their name (for autocomplete).</summary>
    Task<IReadOnlyList<EditorOption>> SearchEditorsAsync(
        string? query, int take = 25, CancellationToken ct = default);

    /// <summary>Top editors ranked by total views generated.</summary>
    Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(int take = 10, CancellationToken ct = default);
}
