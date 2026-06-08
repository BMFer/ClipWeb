using System.Text;
using CLIPWEB.Application.Campaigns;
using Discord;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>The public <c>/campaigns</c> command — lists active campaigns.</summary>
public class CampaignsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ICampaignService _campaigns;

    public CampaignsModule(ICampaignService campaigns) => _campaigns = campaigns;

    [SlashCommand("campaigns", "List the currently active campaigns.")]
    public async Task ListAsync()
    {
        var active = await _campaigns.GetActiveCampaignsAsync();
        if (active.Count == 0)
        {
            await RespondAsync("There are no active campaigns right now. Check back soon!", ephemeral: true);
            return;
        }

        var sb = new StringBuilder();
        foreach (var c in active.Take(25))
        {
            sb.Append("**").Append(c.Name).Append("**");
            if (c.EndDateUtc is { } end)
                sb.Append(" · ends ").Append(end.ToString("yyyy-MM-dd"));
            sb.AppendLine();

            var summary = c.Description.Length > 100 ? c.Description[..97] + "..." : c.Description;
            if (!string.IsNullOrWhiteSpace(summary))
                sb.AppendLine(summary);
            sb.AppendLine();
        }

        var embed = new EmbedBuilder()
            .WithTitle($"Active campaigns ({active.Count})")
            .WithColor(new Color(0x5865F2))
            .WithDescription(sb.ToString().Trim())
            .WithFooter("Use /campaign details to see a specific campaign.")
            .Build();

        await RespondAsync(embed: embed, ephemeral: true);
    }
}
