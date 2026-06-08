using CLIPWEB.Application.Commands.Autocomplete;
using CLIPWEB.Application.Submissions;
using Discord;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>Editor submission commands (<c>/submit …</c>).</summary>
[Group("submit", "Submit your work to a campaign")]
public class SubmitModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ISubmissionService _submissions;

    public SubmitModule(ISubmissionService submissions) => _submissions = submissions;

    [SlashCommand("clip", "Submit a clip to a campaign for review.")]
    public async Task ClipAsync(
        [Summary("campaign", "Campaign to submit to"), Autocomplete(typeof(ActiveCampaignAutocompleteHandler))] string campaign,
        [Summary("url", "Link to your clip")] string url,
        [Summary("notes", "Optional notes for the reviewer")] string? notes = null)
    {
        if (!Guid.TryParse(campaign, out var campaignId))
        {
            await RespondAsync("Pick a campaign from the autocomplete list.", ephemeral: true);
            return;
        }

        url = url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            await RespondAsync("That doesn't look like a valid link. Include the full https:// URL.", ephemeral: true);
            return;
        }

        try
        {
            var submission = await _submissions.SubmitClipAsync(
                Context.User.Id, Context.User.Username, campaignId, url, NullIfBlank(notes));

            var embed = new EmbedBuilder()
                .WithTitle("Clip submitted 🎬")
                .WithColor(Color.Green)
                .WithDescription("Your clip is now in the review queue. You'll be notified once it's reviewed.")
                .AddField("Clip", submission.ClipUrl)
                .WithFooter($"Submission id: {submission.Id}")
                .Build();

            await RespondAsync(embed: embed, ephemeral: true);
        }
        catch (InvalidOperationException ex)
        {
            await RespondAsync(ex.Message, ephemeral: true);
        }
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
