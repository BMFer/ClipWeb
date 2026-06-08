using CLIPWEB.Core.Entities;

namespace CLIPWEB.Application.Campaigns;

/// <summary>
/// Brand and campaign management used by the Phase 3 commands.
/// </summary>
public interface ICampaignService
{
    Task<Brand> CreateBrandAsync(
        string name, string? websiteUrl, string? contactEmail, CancellationToken ct = default);

    Task<Brand?> GetBrandAsync(Guid id, CancellationToken ct = default);

    /// <summary>Brands whose name contains <paramref name="query"/> (for autocomplete).</summary>
    Task<IReadOnlyList<Brand>> SearchBrandsAsync(
        string? query, int take = 25, CancellationToken ct = default);

    Task<Campaign> CreateCampaignAsync(
        Guid brandId, string name, string description,
        DateTime startUtc, DateTime? endUtc,
        string? sourceContentUrl, string? styleGuideUrl, CancellationToken ct = default);

    /// <summary>Closes a campaign (marks inactive, stamps end date). Null if not found.</summary>
    Task<Campaign?> CloseCampaignAsync(Guid campaignId, CancellationToken ct = default);

    Task<Campaign?> GetCampaignAsync(Guid campaignId, CancellationToken ct = default);

    Task<IReadOnlyList<Campaign>> GetActiveCampaignsAsync(CancellationToken ct = default);

    /// <summary>Campaigns whose name contains <paramref name="query"/> (for autocomplete).</summary>
    Task<IReadOnlyList<Campaign>> SearchCampaignsAsync(
        string? query, bool activeOnly, int take = 25, CancellationToken ct = default);
}
