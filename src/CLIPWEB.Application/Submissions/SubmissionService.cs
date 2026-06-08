using CLIPWEB.Application.Onboarding;
using CLIPWEB.Core.Entities;
using CLIPWEB.Core.Enums;
using CLIPWEB.Core.Interfaces;

namespace CLIPWEB.Application.Submissions;

/// <inheritdoc />
public class SubmissionService : ISubmissionService
{
    private readonly IRepository<ClipSubmission> _submissions;
    private readonly IRepository<Campaign> _campaigns;
    private readonly IRepository<EditorProfile> _editors;
    private readonly IEditorOnboardingService _onboarding;

    public SubmissionService(
        IRepository<ClipSubmission> submissions,
        IRepository<Campaign> campaigns,
        IRepository<EditorProfile> editors,
        IEditorOnboardingService onboarding)
    {
        _submissions = submissions;
        _campaigns = campaigns;
        _editors = editors;
        _onboarding = onboarding;
    }

    public async Task<ClipSubmission> SubmitClipAsync(
        ulong discordUserId, string discordUsername,
        Guid campaignId, string clipUrl, string? notes, CancellationToken ct = default)
    {
        var campaign = (await _campaigns.ListAsync(c => c.Id == campaignId, ct)).FirstOrDefault()
            ?? throw new InvalidOperationException("That campaign could not be found.");

        if (!campaign.IsActive)
            throw new InvalidOperationException($"Campaign **{campaign.Name}** is closed and isn't accepting submissions.");

        var editor = await _onboarding.GetOrCreateProfileAsync(discordUserId, discordUsername, ct);

        var submission = new ClipSubmission
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            EditorProfileId = editor.Id,
            ClipUrl = clipUrl,
            Notes = notes,
            Status = SubmissionStatus.Pending,
            SubmittedAtUtc = DateTime.UtcNow
        };
        await _submissions.AddAsync(submission, ct);
        await _submissions.SaveChangesAsync(ct);
        return submission;
    }

    public async Task<IReadOnlyList<SubmissionSummary>> GetPendingAsync(
        Guid? campaignId = null, int take = 25, CancellationToken ct = default)
    {
        var pending = campaignId is { } id
            ? await _submissions.ListAsync(s => s.Status == SubmissionStatus.Pending && s.CampaignId == id, ct)
            : await _submissions.ListAsync(s => s.Status == SubmissionStatus.Pending, ct);

        var summaries = await BuildSummariesAsync(pending, ct);
        return summaries.Take(take).ToList();
    }

    public async Task<IReadOnlyList<SubmissionSummary>> SearchPendingAsync(
        string? query, int take = 25, CancellationToken ct = default)
    {
        var all = await GetPendingAsync(null, int.MaxValue, ct);
        if (string.IsNullOrWhiteSpace(query))
            return all.Take(take).ToList();

        return all
            .Where(s => s.CampaignName.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || s.EditorName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(take)
            .ToList();
    }

    public async Task<SubmissionSummary?> SetStatusAsync(
        Guid submissionId, SubmissionStatus status, CancellationToken ct = default)
    {
        var submission = (await _submissions.ListAsync(s => s.Id == submissionId, ct)).FirstOrDefault();
        if (submission is null)
            return null;

        submission.Status = status;
        _submissions.Update(submission);
        await _submissions.SaveChangesAsync(ct);

        return (await BuildSummariesAsync([submission], ct)).Single();
    }

    public async Task<IReadOnlyList<SubmissionSummary>> GetApprovedForEditorAsync(
        ulong discordUserId, string? query = null, int take = 25, CancellationToken ct = default)
    {
        var editor = (await _editors.ListAsync(e => e.DiscordUserId == discordUserId, ct)).FirstOrDefault();
        if (editor is null)
            return [];

        var approved = await _submissions.ListAsync(
            s => s.EditorProfileId == editor.Id && s.Status == SubmissionStatus.Approved, ct);

        var summaries = await BuildSummariesAsync(approved, ct);
        if (!string.IsNullOrWhiteSpace(query))
            summaries = summaries
                .Where(s => s.CampaignName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        return summaries.Take(take).ToList();
    }

    private async Task<List<SubmissionSummary>> BuildSummariesAsync(
        IReadOnlyList<ClipSubmission> submissions, CancellationToken ct)
    {
        if (submissions.Count == 0)
            return [];

        var campaignIds = submissions.Select(s => s.CampaignId).Distinct().ToList();
        var editorIds = submissions.Select(s => s.EditorProfileId).Distinct().ToList();

        var campaigns = (await _campaigns.ListAsync(c => campaignIds.Contains(c.Id), ct))
            .ToDictionary(c => c.Id);
        var editors = (await _editors.ListAsync(e => editorIds.Contains(e.Id), ct))
            .ToDictionary(e => e.Id);

        return submissions
            .Select(s =>
            {
                campaigns.TryGetValue(s.CampaignId, out var campaign);
                editors.TryGetValue(s.EditorProfileId, out var editor);
                var editorName = editor?.PreferredName
                    ?? editor?.DiscordUsername
                    ?? "Unknown editor";

                return new SubmissionSummary(
                    s.Id,
                    campaign?.Name ?? "Unknown campaign",
                    editorName,
                    editor?.DiscordUserId ?? 0,
                    s.ClipUrl,
                    s.Notes,
                    s.Status,
                    s.SubmittedAtUtc);
            })
            .OrderBy(s => s.SubmittedAtUtc)
            .ToList();
    }
}
