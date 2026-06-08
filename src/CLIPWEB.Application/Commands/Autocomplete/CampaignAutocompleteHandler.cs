using CLIPWEB.Application.Campaigns;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace CLIPWEB.Application.Commands.Autocomplete;

/// <summary>Suggests campaigns (active and closed) by name; value is the campaign id.</summary>
public class CampaignAutocompleteHandler : AutocompleteHandler
{
    protected virtual bool ActiveOnly => false;

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var service = services.GetRequiredService<ICampaignService>();
        var query = interaction.Data.Current.Value?.ToString();

        var campaigns = await service.SearchCampaignsAsync(query, ActiveOnly);
        var results = campaigns.Select(c =>
            new AutocompleteResult(BrandAutocompleteHandler.Truncate(c.Name), c.Id.ToString()));

        return AutocompletionResult.FromSuccess(results);
    }
}

/// <summary>Suggests only active campaigns (e.g. for /campaign close).</summary>
public class ActiveCampaignAutocompleteHandler : CampaignAutocompleteHandler
{
    protected override bool ActiveOnly => true;
}
