using Discord;

namespace CLIPWEB.Application.Onboarding;

/// <summary>
/// The official CLIPWEB welcome message (InitSpecs.md §6), shared by the
/// <c>/welcome</c> command and the member-join handler.
/// </summary>
public static class WelcomeMessage
{
    public const string Text =
        """
        This is where brands, campaigns, clipping networks, and editors connect.

        Your job here is simple:

        Find active campaigns.
        Create strong clips.
        Submit your work.
        Publish approved content.
        Track the views you generate.

        CLIPWEB keeps score so your work does not disappear into the noise.

        Every clip you submit, every post you publish, and every view you generate builds your editor profile inside the network.

        Start by completing the onboarding survey with **/survey**.

        After that, use **/campaigns** to see what is active.

        Welcome to the web.
        Now start clipping.
        """;

    /// <summary>Builds the welcome message as a rich embed.</summary>
    public static Embed BuildEmbed() => new EmbedBuilder()
        .WithTitle("Welcome to CLIPWEB")
        .WithDescription(Text)
        .WithColor(new Color(0x5865F2))
        .Build();
}
