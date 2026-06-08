using CLIPWEB.Application.Authorization;
using CLIPWEB.Application.Commands.Autocomplete;
using CLIPWEB.Application.Reporting;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>Manager reporting commands (<c>/report …</c>).</summary>
[Group("report", "Performance reports")]
[RequireManager]
public class ReportModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IReportingService _reporting;

    public ReportModule(IReportingService reporting) => _reporting = reporting;

    [SlashCommand("editor", "Show an editor's performance.")]
    public async Task EditorAsync(
        [Summary("editor", "Editor to report on"), Autocomplete(typeof(EditorAutocompleteHandler))] string editor)
    {
        if (!Guid.TryParse(editor, out var editorId) ||
            await _reporting.GetEditorStatsAsync(editorId) is not { } stats)
        {
            await RespondAsync("Pick an editor from the autocomplete list.", ephemeral: true);
            return;
        }

        await RespondAsync(embed: ReportEmbeds.Editor(stats, $"Editor report · {stats.EditorName}"), ephemeral: true);
    }

    [SlashCommand("campaign", "Show a campaign's totals.")]
    public async Task CampaignAsync(
        [Summary("campaign", "Campaign to report on"), Autocomplete(typeof(CampaignAutocompleteHandler))] string campaign)
    {
        if (!Guid.TryParse(campaign, out var campaignId) ||
            await _reporting.GetCampaignReportAsync(campaignId) is not { } report)
        {
            await RespondAsync("Pick a campaign from the autocomplete list.", ephemeral: true);
            return;
        }

        await RespondAsync(embed: ReportEmbeds.Campaign(report), ephemeral: true);
    }

    [SlashCommand("brand", "Show a brand's performance across its campaigns.")]
    public async Task BrandAsync(
        [Summary("brand", "Brand to report on"), Autocomplete(typeof(BrandAutocompleteHandler))] string brand)
    {
        if (!Guid.TryParse(brand, out var brandId) ||
            await _reporting.GetBrandSummaryAsync(brandId) is not { } summary)
        {
            await RespondAsync("Pick a brand from the autocomplete list.", ephemeral: true);
            return;
        }

        await RespondAsync(embed: ReportEmbeds.Brand(summary), ephemeral: true);
    }
}
