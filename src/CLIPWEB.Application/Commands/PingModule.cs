using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>
/// Diagnostic slash commands used to verify the bot is online and dispatching
/// interactions. Acts as the smoke test for the Phase 1 foundation wiring.
/// </summary>
public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Check that CLIPWEB is online and responding.")]
    public async Task PingAsync()
    {
        var latencyMs = Context.Client.Latency;
        await RespondAsync(
            $"🟢 CLIPWEB is online. Gateway latency: {latencyMs} ms.",
            ephemeral: true);
    }
}
