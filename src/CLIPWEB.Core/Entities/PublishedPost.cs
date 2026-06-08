namespace CLIPWEB.Core.Entities;

/// <summary>
/// A published social post linked to an approved clip submission, with view tracking.
/// </summary>
public class PublishedPost
{
    public Guid Id { get; set; }
    public Guid ClipSubmissionId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string PostUrl { get; set; } = string.Empty;
    public long Views { get; set; }
    public long? Likes { get; set; }
    public long? Comments { get; set; }
    public long? Shares { get; set; }
    public DateTime PostedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }

    /// <summary>The clip submission this post was published from.</summary>
    public ClipSubmission? ClipSubmission { get; set; }
}
