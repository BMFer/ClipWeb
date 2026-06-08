using CLIPWEB.Core.Entities;
using CLIPWEB.Core.Enums;
using CLIPWEB.Core.Interfaces;

namespace CLIPWEB.Application.Submissions;

/// <inheritdoc />
public class PublishedPostService : IPublishedPostService
{
    private readonly IRepository<PublishedPost> _posts;
    private readonly IRepository<ClipSubmission> _submissions;
    private readonly IRepository<EditorProfile> _editors;

    public PublishedPostService(
        IRepository<PublishedPost> posts,
        IRepository<ClipSubmission> submissions,
        IRepository<EditorProfile> editors)
    {
        _posts = posts;
        _submissions = submissions;
        _editors = editors;
    }

    public async Task<PublishedPost> AddPostAsync(
        ulong discordUserId,
        Guid submissionId,
        string platform,
        string postUrl,
        long views,
        long? likes,
        long? comments,
        long? shares,
        CancellationToken ct = default)
    {
        var submission = (await _submissions.ListAsync(s => s.Id == submissionId, ct)).FirstOrDefault()
            ?? throw new InvalidOperationException("That clip submission could not be found.");

        var editor = (await _editors.ListAsync(e => e.DiscordUserId == discordUserId, ct)).FirstOrDefault();
        if (editor is null || submission.EditorProfileId != editor.Id)
            throw new InvalidOperationException("You can only add posts to your own clips.");

        if (submission.Status != SubmissionStatus.Approved)
            throw new InvalidOperationException("Only approved clips can have published posts added.");

        var now = DateTime.UtcNow;
        var post = new PublishedPost
        {
            Id = Guid.NewGuid(),
            ClipSubmissionId = submission.Id,
            Platform = platform,
            PostUrl = postUrl,
            Views = views,
            Likes = likes,
            Comments = comments,
            Shares = shares,
            PostedAtUtc = now,
            LastUpdatedAtUtc = now
        };
        await _posts.AddAsync(post, ct);
        await _posts.SaveChangesAsync(ct);
        return post;
    }
}
