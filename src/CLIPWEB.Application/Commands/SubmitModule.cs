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
    private readonly IPublishedPostService _posts;

    public SubmitModule(ISubmissionService submissions, IPublishedPostService posts)
    {
        _submissions = submissions;
        _posts = posts;
    }

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

    [SlashCommand("post", "Log a published post for one of your approved clips.")]
    public async Task PostAsync(
        [Summary("clip", "Your approved clip"), Autocomplete(typeof(PostableSubmissionAutocompleteHandler))] string clip,
        [Summary("platform", "Where you published it")] SocialPlatform platform,
        [Summary("url", "Link to the published post")] string url,
        [Summary("views", "View count"), MinValue(0)] long views,
        [Summary("likes", "Likes (optional)"), MinValue(0)] long? likes = null,
        [Summary("comments", "Comments (optional)"), MinValue(0)] long? comments = null,
        [Summary("shares", "Shares (optional)"), MinValue(0)] long? shares = null)
    {
        if (!Guid.TryParse(clip, out var submissionId))
        {
            await RespondAsync("Pick one of your approved clips from the autocomplete list.", ephemeral: true);
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
            var post = await _posts.AddPostAsync(
                Context.User.Id, submissionId, platform.ToDisplayName(), url,
                views, likes, comments, shares);

            var embed = new EmbedBuilder()
                .WithTitle("Post logged 📈")
                .WithColor(Color.Green)
                .WithDescription("Nice work! Your published post is now tracked.")
                .AddField("Platform", post.Platform, inline: true)
                .AddField("Views", post.Views.ToString("N0"), inline: true)
                .AddField("Post", post.PostUrl);

            if (post.Likes is { } l) embed.AddField("Likes", l.ToString("N0"), inline: true);
            if (post.Comments is { } c) embed.AddField("Comments", c.ToString("N0"), inline: true);
            if (post.Shares is { } sh) embed.AddField("Shares", sh.ToString("N0"), inline: true);

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }
        catch (InvalidOperationException ex)
        {
            await RespondAsync(ex.Message, ephemeral: true);
        }
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
