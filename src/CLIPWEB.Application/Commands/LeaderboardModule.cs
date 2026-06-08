using CLIPWEB.Application.Reporting;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>The public <c>/leaderboard</c> command — top editors by views.</summary>
public class LeaderboardModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IReportingService _reporting;

    public LeaderboardModule(IReportingService reporting) => _reporting = reporting;

    [SlashCommand("leaderboard", "See the top editors by views generated.")]
    public async Task LeaderboardAsync()
    {
        var entries = await _reporting.GetLeaderboardAsync(10);
        if (entries.Count == 0)
        {
            await RespondAsync("No published posts are tracked yet — the leaderboard is empty.", ephemeral: true);
            return;
        }

        await RespondAsync(embed: ReportEmbeds.Leaderboard(entries));
    }
}
