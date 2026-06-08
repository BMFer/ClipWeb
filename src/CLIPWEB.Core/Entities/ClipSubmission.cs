using CLIPWEB.Core.Enums;

namespace CLIPWEB.Core.Entities;

/// <summary>
/// A clip submitted by an editor against a campaign, pending review.
/// </summary>
public class ClipSubmission
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid EditorProfileId { get; set; }
    public string ClipUrl { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAtUtc { get; set; }

    // --- Review feedback (set when a manager acts on the submission) ---

    /// <summary>The reviewer's note/reason, if one was given.</summary>
    public string? ReviewerNote { get; set; }

    /// <summary>When the submission was last reviewed.</summary>
    public DateTime? ReviewedAtUtc { get; set; }

    /// <summary>Discord id of the reviewer who last changed the status.</summary>
    public ulong? ReviewedByDiscordUserId { get; set; }

    /// <summary>The campaign this clip was submitted to.</summary>
    public Campaign? Campaign { get; set; }

    /// <summary>The editor who submitted this clip.</summary>
    public EditorProfile? EditorProfile { get; set; }

    /// <summary>Published social posts derived from this clip once approved.</summary>
    public ICollection<PublishedPost> PublishedPosts { get; set; } = new List<PublishedPost>();
}
