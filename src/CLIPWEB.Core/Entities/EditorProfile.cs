namespace CLIPWEB.Core.Entities;

/// <summary>
/// A community editor/clipper, linked to their Discord identity.
/// </summary>
public class EditorProfile
{
    public Guid Id { get; set; }
    public ulong DiscordUserId { get; set; }
    public string DiscordUsername { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    // --- Onboarding survey answers (see InitSpecs.md §7) ---

    /// <summary>Q1 — what name to call the editor.</summary>
    public string? PreferredName { get; set; }

    /// <summary>Q2 — editor's time zone.</summary>
    public string? TimeZone { get; set; }

    /// <summary>Q3 — editing software (CapCut, Premiere, etc.).</summary>
    public string? EditingSoftware { get; set; }

    /// <summary>Q4 — platforms they mainly post on.</summary>
    public string? PrimaryPlatform { get; set; }

    /// <summary>Q5 — experience level.</summary>
    public string? ExperienceLevel { get; set; }

    /// <summary>Q6 — content they clip best.</summary>
    public string? ContentNiche { get; set; }

    /// <summary>Q7 — clips they can realistically make per week.</summary>
    public int? ClipsPerWeek { get; set; }

    /// <summary>Q8 — whether they can publish on their own accounts (Yes/No/Sometimes).</summary>
    public string? CanSelfPublish { get; set; }

    /// <summary>Q9 — portfolio or example post link.</summary>
    public string? PortfolioUrl { get; set; }

    /// <summary>Q10 — preferred way for campaign managers to contact them.</summary>
    public string? ContactPreference { get; set; }

    /// <summary>True once the full onboarding survey has been completed.</summary>
    public bool SurveyCompleted { get; set; }

    /// <summary>When the survey was completed, if it has been.</summary>
    public DateTime? SurveyCompletedAtUtc { get; set; }

    /// <summary>Clip submissions made by this editor.</summary>
    public ICollection<ClipSubmission> Submissions { get; set; } = new List<ClipSubmission>();
}
