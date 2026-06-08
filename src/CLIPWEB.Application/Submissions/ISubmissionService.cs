using CLIPWEB.Core.Entities;
using CLIPWEB.Core.Enums;

namespace CLIPWEB.Application.Submissions;

/// <summary>
/// Clip submission workflow: submit, review queue, and approve/reject/revision.
/// </summary>
public interface ISubmissionService
{
    /// <summary>
    /// Submits a clip against an active campaign, creating the editor's profile
    /// if they don't have one yet. Throws <see cref="InvalidOperationException"/>
    /// if the campaign is missing or closed.
    /// </summary>
    Task<ClipSubmission> SubmitClipAsync(
        ulong discordUserId, string discordUsername,
        Guid campaignId, string clipUrl, string? notes, CancellationToken ct = default);

    /// <summary>Pending submissions, newest-last, optionally filtered by campaign.</summary>
    Task<IReadOnlyList<SubmissionSummary>> GetPendingAsync(
        Guid? campaignId = null, int take = 25, CancellationToken ct = default);

    /// <summary>Pending submissions matching a query against campaign/editor name.</summary>
    Task<IReadOnlyList<SubmissionSummary>> SearchPendingAsync(
        string? query, int take = 25, CancellationToken ct = default);

    /// <summary>Sets a submission's status; returns the updated view, or null if not found.</summary>
    Task<SubmissionSummary?> SetStatusAsync(
        Guid submissionId, SubmissionStatus status, CancellationToken ct = default);
}
