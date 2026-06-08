namespace CLIPWEB.Core.Entities;

/// <summary>
/// A campaign a brand runs to source community clips.
/// </summary>
public class Campaign
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SourceContentUrl { get; set; }
    public string? StyleGuideUrl { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public bool IsActive { get; set; }

    /// <summary>The brand that owns this campaign.</summary>
    public Brand? Brand { get; set; }

    /// <summary>Clip submissions made against this campaign.</summary>
    public ICollection<ClipSubmission> Submissions { get; set; } = new List<ClipSubmission>();
}
