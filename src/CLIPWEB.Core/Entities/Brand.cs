namespace CLIPWEB.Core.Entities;

/// <summary>
/// A brand that runs clipping campaigns through the network.
/// </summary>
public class Brand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? ContactEmail { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Campaigns launched by this brand.</summary>
    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
