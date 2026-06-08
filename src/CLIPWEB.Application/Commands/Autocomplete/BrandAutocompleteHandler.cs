using CLIPWEB.Application.Campaigns;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace CLIPWEB.Application.Commands.Autocomplete;

/// <summary>Suggests brands by name; the option value is the brand id.</summary>
public class BrandAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var service = services.GetRequiredService<ICampaignService>();
        var query = interaction.Data.Current.Value?.ToString();

        var brands = await service.SearchBrandsAsync(query);
        var results = brands.Select(b =>
            new AutocompleteResult(Truncate(b.Name), b.Id.ToString()));

        return AutocompletionResult.FromSuccess(results);
    }

    internal static string Truncate(string value)
        => value.Length <= 100 ? value : value[..97] + "...";
}
