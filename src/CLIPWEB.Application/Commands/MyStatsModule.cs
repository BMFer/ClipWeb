using CLIPWEB.Application.Reporting;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>The public <c>/mystats</c> command — an editor's own performance.</summary>
public class MyStatsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IReportingService _reporting;

    public MyStatsModule(IReportingService reporting) => _reporting = reporting;

    [SlashCommand("mystats", "See your own editor stats.")]
    public async Task MyStatsAsync()
    {
        var stats = await _reporting.GetEditorStatsByDiscordIdAsync(Context.User.Id);
        if (stats is null)
        {
            await RespondAsync(
                "You don't have any stats yet. Submit a clip with **/submit clip** to get started!",
                ephemeral: true);
            return;
        }

        await RespondAsync(embed: ReportEmbeds.Editor(stats, "Your CLIPWEB stats"), ephemeral: true);
    }
}
