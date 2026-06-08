using System.Text;
using CLIPWEB.Application.Authorization;
using CLIPWEB.Application.Commands.Autocomplete;
using CLIPWEB.Application.Submissions;
using CLIPWEB.Core.Enums;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace CLIPWEB.Application.Commands;

/// <summary>Reviewer commands (<c>/submission …</c>) — manager only.</summary>
[Group("submission", "Review clip submissions")]
[RequireManager]
public class SubmissionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ISubmissionService _submissions;
    private readonly ILogger<SubmissionModule> _logger;

    public SubmissionModule(ISubmissionService submissions, ILogger<SubmissionModule> logger)
    {
        _submissions = submissions;
        _logger = logger;
    }

    [SlashCommand("review", "List pending submissions awaiting review.")]
    public async Task ReviewAsync(
        [Summary("campaign", "Filter by campaign"), Autocomplete(typeof(ActiveCampaignAutocompleteHandler))] string? campaign = null)
    {
        Guid? campaignId = Guid.TryParse(campaign, out var id) ? id : null;
        var pending = await _submissions.GetPendingAsync(campaignId);

        if (pending.Count == 0)
        {
            await RespondAsync("🎉 No pending submissions. The queue is clear.", ephemeral: true);
            return;
        }

        var sb = new StringBuilder();
        foreach (var s in pending)
        {
            sb.Append("**").Append(s.CampaignName).Append("** · ").Append(s.EditorName)
              .Append(" · ").Append(s.SubmittedAtUtc.ToString("yyyy-MM-dd")).AppendLine();
            sb.Append('<').Append(s.ClipUrl).Append('>').AppendLine();
            if (!string.IsNullOrWhiteSpace(s.Notes))
                sb.Append("> ").Append(s.Notes).AppendLine();
            sb.AppendLine();
        }

        var embed = new EmbedBuilder()
            .WithTitle($"Pending submissions ({pending.Count})")
            .WithColor(new Color(0xFEE75C))
            .WithDescription(sb.ToString().Trim())
            .WithFooter("Use /submission approve, reject, or revision.")
            .Build();

        await RespondAsync(embed: embed, ephemeral: true);
    }

    [SlashCommand("approve", "Approve a pending submission.")]
    public Task ApproveAsync(
        [Summary("submission", "Submission to approve"), Autocomplete(typeof(PendingSubmissionAutocompleteHandler))] string submission,
        [Summary("note", "Optional note to the editor")] string? note = null)
        => ApplyStatusAsync(submission, SubmissionStatus.Approved, note);

    [SlashCommand("reject", "Reject a pending submission.")]
    public Task RejectAsync(
        [Summary("submission", "Submission to reject"), Autocomplete(typeof(PendingSubmissionAutocompleteHandler))] string submission,
        [Summary("reason", "Optional reason for the editor")] string? reason = null)
        => ApplyStatusAsync(submission, SubmissionStatus.Rejected, reason);

    [SlashCommand("revision", "Send a submission back for revision.")]
    public Task RevisionAsync(
        [Summary("submission", "Submission needing revision"), Autocomplete(typeof(PendingSubmissionAutocompleteHandler))] string submission,
        [Summary("note", "What needs changing")] string? note = null)
        => ApplyStatusAsync(submission, SubmissionStatus.NeedsRevision, note);

    private async Task ApplyStatusAsync(string submissionRaw, SubmissionStatus status, string? note)
    {
        if (!Guid.TryParse(submissionRaw, out var submissionId))
        {
            await RespondAsync("Pick a submission from the autocomplete list.", ephemeral: true);
            return;
        }

        var result = await _submissions.SetStatusAsync(submissionId, status);
        if (result is null)
        {
            await RespondAsync("That submission could not be found.", ephemeral: true);
            return;
        }

        await NotifyEditorAsync(result, status, note);

        var (verb, _) = Describe(status);
        var confirm = $"{StatusEmoji(status)} **{result.CampaignName}** submission by {result.EditorName} was {verb}.";
        await RespondAsync(confirm, ephemeral: true);
    }

    private async Task NotifyEditorAsync(SubmissionSummary submission, SubmissionStatus status, string? note)
    {
        if (submission.EditorDiscordUserId == 0)
            return;

        try
        {
            var user = await Context.Client.GetUserAsync(submission.EditorDiscordUserId);
            if (user is null)
                return;

            var (_, headline) = Describe(status);
            var embed = new EmbedBuilder()
                .WithTitle(headline)
                .WithColor(StatusColor(status))
                .WithDescription($"Your clip for **{submission.CampaignName}** {VerbPhrase(status)}.")
                .AddField("Clip", submission.ClipUrl);

            if (!string.IsNullOrWhiteSpace(note))
                embed.AddField("Note from reviewer", note);
            if (status == SubmissionStatus.Approved)
                embed.AddField("Next step", "Publish it, then log the post with **/submit post**.");

            var dm = await user.CreateDMChannelAsync();
            await dm.SendMessageAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            // The editor may have DMs closed — don't fail the review.
            _logger.LogWarning(ex,
                "Could not DM editor {UserId} about submission {SubmissionId}.",
                submission.EditorDiscordUserId, submission.SubmissionId);
        }
    }

    private static (string Verb, string Headline) Describe(SubmissionStatus status) => status switch
    {
        SubmissionStatus.Approved => ("approved", "Your clip was approved ✅"),
        SubmissionStatus.Rejected => ("rejected", "Your clip was not accepted"),
        SubmissionStatus.NeedsRevision => ("sent back for revision", "Your clip needs a revision"),
        _ => ("updated", "Your clip status changed")
    };

    private static string VerbPhrase(SubmissionStatus status) => status switch
    {
        SubmissionStatus.Approved => "has been **approved**",
        SubmissionStatus.Rejected => "was **not accepted** this time",
        SubmissionStatus.NeedsRevision => "needs a **revision**",
        _ => "was updated"
    };

    private static string StatusEmoji(SubmissionStatus status) => status switch
    {
        SubmissionStatus.Approved => "✅",
        SubmissionStatus.Rejected => "⛔",
        SubmissionStatus.NeedsRevision => "✏️",
        _ => "ℹ️"
    };

    private static Color StatusColor(SubmissionStatus status) => status switch
    {
        SubmissionStatus.Approved => Color.Green,
        SubmissionStatus.Rejected => Color.Red,
        SubmissionStatus.NeedsRevision => Color.Orange,
        _ => Color.LightGrey
    };
}
