namespace CLIPWEB.Core.Enums;

/// <summary>
/// Lifecycle state of a <see cref="Entities.ClipSubmission"/>.
/// </summary>
public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected,
    NeedsRevision
}
