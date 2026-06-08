namespace CLIPWEB.Core.Entities;

/// <summary>
/// A community editor/clipper, linked to their Discord identity.
/// </summary>
public class EditorProfile
{
    public Guid Id { get; set; }
    public ulong DiscordUserId { get; set; }
    public string DiscordUsername { get; set; } = string.Empty;
    public string? PreferredName { get; set; }
    public string? TimeZone { get; set; }
    public string? PrimaryPlatform { get; set; }
    public bool SurveyCompleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Clip submissions made by this editor.</summary>
    public ICollection<ClipSubmission> Submissions { get; set; } = new List<ClipSubmission>();
}
