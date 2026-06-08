using System.Globalization;
using CLIPWEB.Application.Authorization;
using CLIPWEB.Application.Campaigns;
using CLIPWEB.Application.Commands.Autocomplete;
using CLIPWEB.Core.Entities;
using Discord;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands;

/// <summary>Campaign management commands (<c>/campaign …</c>).</summary>
[Group("campaign", "Manage campaigns")]
public class CampaignModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ICampaignService _campaigns;

    public CampaignModule(ICampaignService campaigns) => _campaigns = campaigns;

    [SlashCommand("create", "Create a campaign for a brand.")]
    [RequireManager]
    public async Task CreateAsync(
        [Summary("brand", "Brand running the campaign"), Autocomplete(typeof(BrandAutocompleteHandler))] string brand,
        [Summary("name", "Campaign name")] string name,
        [Summary("description", "What clippers should make")] string description,
        [Summary("start", "Start date (YYYY-MM-DD, default today)")] string? start = null,
        [Summary("end", "End date (YYYY-MM-DD, optional)")] string? end = null,
        [Summary("source", "Source content URL")] string? source = null,
        [Summary("style", "Style guide URL")] string? style = null)
    {
        if (!Guid.TryParse(brand, out var brandId) ||
            await _campaigns.GetBrandAsync(brandId) is not { } resolvedBrand)
        {
            await RespondAsync("Pick a brand from the autocomplete list.", ephemeral: true);
            return;
        }

        if (!TryParseDate(start, DateTime.UtcNow.Date, out var startUtc, out var startError))
        {
            await RespondAsync(startError, ephemeral: true);
            return;
        }

        DateTime? endUtc = null;
        if (!string.IsNullOrWhiteSpace(end))
        {
            if (!TryParseDate(end, default, out var parsedEnd, out var endError))
            {
                await RespondAsync(endError, ephemeral: true);
                return;
            }
            if (parsedEnd < startUtc)
            {
                await RespondAsync("The end date can't be before the start date.", ephemeral: true);
                return;
            }
            endUtc = parsedEnd;
        }

        var campaign = await _campaigns.CreateCampaignAsync(
            brandId, name.Trim(), description.Trim(), startUtc, endUtc,
            NullIfBlank(source), NullIfBlank(style));

        await RespondAsync(embed: BuildCampaignEmbed(campaign, resolvedBrand, "Campaign created"),
            ephemeral: true);
    }

    [SlashCommand("close", "Close an active campaign.")]
    [RequireManager]
    public async Task CloseAsync(
        [Summary("campaign", "Campaign to close"), Autocomplete(typeof(ActiveCampaignAutocompleteHandler))] string campaign)
    {
        if (!Guid.TryParse(campaign, out var campaignId))
        {
            await RespondAsync("Pick a campaign from the autocomplete list.", ephemeral: true);
            return;
        }

        var closed = await _campaigns.CloseCampaignAsync(campaignId);
        if (closed is null)
        {
            await RespondAsync("That campaign could not be found.", ephemeral: true);
            return;
        }

        await RespondAsync(
            $"🔒 Campaign **{closed.Name}** is now closed.", ephemeral: true);
    }

    [SlashCommand("details", "Show the details of a campaign.")]
    public async Task DetailsAsync(
        [Summary("campaign", "Campaign to view"), Autocomplete(typeof(CampaignAutocompleteHandler))] string campaign)
    {
        if (!Guid.TryParse(campaign, out var campaignId) ||
            await _campaigns.GetCampaignAsync(campaignId) is not { } found)
        {
            await RespondAsync("Pick a campaign from the autocomplete list.", ephemeral: true);
            return;
        }

        var brand = await _campaigns.GetBrandAsync(found.BrandId);
        await RespondAsync(embed: BuildCampaignEmbed(found, brand, found.Name));
    }

    private static Embed BuildCampaignEmbed(Campaign campaign, Brand? brand, string title)
    {
        var builder = new EmbedBuilder()
            .WithTitle(title)
            .WithColor(campaign.IsActive ? Color.Green : Color.LightGrey)
            .WithDescription(campaign.Description)
            .AddField("Brand", brand?.Name ?? "Unknown", inline: true)
            .AddField("Status", campaign.IsActive ? "🟢 Active" : "🔒 Closed", inline: true)
            .AddField("Starts", FormatDate(campaign.StartDateUtc), inline: true)
            .AddField("Ends", campaign.EndDateUtc is { } e ? FormatDate(e) : "—", inline: true);

        if (!string.IsNullOrWhiteSpace(campaign.SourceContentUrl))
            builder.AddField("Source", campaign.SourceContentUrl);
        if (!string.IsNullOrWhiteSpace(campaign.StyleGuideUrl))
            builder.AddField("Style guide", campaign.StyleGuideUrl);

        return builder.WithFooter($"Campaign id: {campaign.Id}").Build();
    }

    private static string FormatDate(DateTime date) => date.ToString("yyyy-MM-dd");

    private static bool TryParseDate(string? input, DateTime fallback, out DateTime value, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            value = DateTime.SpecifyKind(fallback, DateTimeKind.Utc);
            return true;
        }

        if (DateTime.TryParseExact(input.Trim(), "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            value = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            return true;
        }

        value = default;
        error = $"`{input}` isn't a valid date. Use the format YYYY-MM-DD.";
        return false;
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
