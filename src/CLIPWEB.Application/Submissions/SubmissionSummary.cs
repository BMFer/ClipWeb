using CLIPWEB.Core.Enums;

namespace CLIPWEB.Application.Submissions;

/// <summary>
/// A flattened view of a clip submission with its related campaign and editor,
/// used by the review queue and autocomplete.
/// </summary>
public record SubmissionSummary(
    Guid SubmissionId,
    string CampaignName,
    string EditorName,
    ulong EditorDiscordUserId,
    string ClipUrl,
    string? Notes,
    SubmissionStatus Status,
    DateTime SubmittedAtUtc);
