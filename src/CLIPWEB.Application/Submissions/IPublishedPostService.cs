using CLIPWEB.Core.Entities;

namespace CLIPWEB.Application.Submissions;

/// <summary>
/// Records published social posts against approved clip submissions.
/// </summary>
public interface IPublishedPostService
{
    /// <summary>
    /// Logs a published post for one of the editor's own approved submissions.
    /// Throws <see cref="InvalidOperationException"/> if the submission is
    /// missing, not owned by the editor, or not approved.
    /// </summary>
    Task<PublishedPost> AddPostAsync(
        ulong discordUserId,
        Guid submissionId,
        string platform,
        string postUrl,
        long views,
        long? likes,
        long? comments,
        long? shares,
        CancellationToken ct = default);
}
