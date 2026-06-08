using CLIPWEB.Application.Onboarding;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>
/// The <c>/welcome</c> command — posts the official CLIPWEB welcome message.
/// </summary>
public class WelcomeModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("welcome", "Show the official CLIPWEB welcome message.")]
    public async Task WelcomeAsync()
    {
        await RespondAsync(embed: WelcomeMessage.BuildEmbed());
    }
}
