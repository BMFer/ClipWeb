using CLIPWEB.Core.Entities;
using CLIPWEB.Core.Interfaces;

namespace CLIPWEB.Application.Campaigns;

/// <inheritdoc />
public class CampaignService : ICampaignService
{
    private readonly IRepository<Brand> _brands;
    private readonly IRepository<Campaign> _campaigns;

    public CampaignService(IRepository<Brand> brands, IRepository<Campaign> campaigns)
    {
        _brands = brands;
        _campaigns = campaigns;
    }

    public async Task<Brand> CreateBrandAsync(
        string name, string? websiteUrl, string? contactEmail, CancellationToken ct = default)
    {
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = name,
            WebsiteUrl = websiteUrl,
            ContactEmail = contactEmail,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _brands.AddAsync(brand, ct);
        await _brands.SaveChangesAsync(ct);
        return brand;
    }

    public async Task<Brand?> GetBrandAsync(Guid id, CancellationToken ct = default)
        => (await _brands.ListAsync(b => b.Id == id, ct)).FirstOrDefault();

    public async Task<IReadOnlyList<Brand>> SearchBrandsAsync(
        string? query, int take = 25, CancellationToken ct = default)
    {
        var brands = string.IsNullOrWhiteSpace(query)
            ? await _brands.ListAsync(ct: ct)
            : await _brands.ListAsync(b => b.Name.ToLower().Contains(query.ToLower()), ct);

        return brands.OrderBy(b => b.Name).Take(take).ToList();
    }

    public async Task<Campaign> CreateCampaignAsync(
        Guid brandId, string name, string description,
        DateTime startUtc, DateTime? endUtc,
        string? sourceContentUrl, string? styleGuideUrl, CancellationToken ct = default)
    {
        var brand = await GetBrandAsync(brandId, ct)
            ?? throw new InvalidOperationException($"Brand {brandId} does not exist.");

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            BrandId = brand.Id,
            Name = name,
            Description = description,
            SourceContentUrl = sourceContentUrl,
            StyleGuideUrl = styleGuideUrl,
            StartDateUtc = startUtc,
            EndDateUtc = endUtc,
            IsActive = true
        };
        await _campaigns.AddAsync(campaign, ct);
        await _campaigns.SaveChangesAsync(ct);
        return campaign;
    }

    public async Task<Campaign?> CloseCampaignAsync(Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await GetCampaignAsync(campaignId, ct);
        if (campaign is null)
            return null;

        campaign.IsActive = false;
        campaign.EndDateUtc ??= DateTime.UtcNow;
        _campaigns.Update(campaign);
        await _campaigns.SaveChangesAsync(ct);
        return campaign;
    }

    public async Task<Campaign?> GetCampaignAsync(Guid campaignId, CancellationToken ct = default)
        => (await _campaigns.ListAsync(c => c.Id == campaignId, ct)).FirstOrDefault();

    public async Task<IReadOnlyList<Campaign>> GetActiveCampaignsAsync(CancellationToken ct = default)
    {
        var active = await _campaigns.ListAsync(c => c.IsActive, ct);
        return active.OrderBy(c => c.StartDateUtc).ToList();
    }

    public async Task<IReadOnlyList<Campaign>> SearchCampaignsAsync(
        string? query, bool activeOnly, int take = 25, CancellationToken ct = default)
    {
        var hasQuery = !string.IsNullOrWhiteSpace(query);
        var lowered = query?.ToLower() ?? string.Empty;

        IReadOnlyList<Campaign> campaigns = (activeOnly, hasQuery) switch
        {
            (true, true) => await _campaigns.ListAsync(
                c => c.IsActive && c.Name.ToLower().Contains(lowered), ct),
            (true, false) => await _campaigns.ListAsync(c => c.IsActive, ct),
            (false, true) => await _campaigns.ListAsync(
                c => c.Name.ToLower().Contains(lowered), ct),
            (false, false) => await _campaigns.ListAsync(ct: ct)
        };

        return campaigns.OrderBy(c => c.Name).Take(take).ToList();
    }
}
