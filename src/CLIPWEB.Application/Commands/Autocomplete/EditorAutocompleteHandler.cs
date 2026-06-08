using CLIPWEB.Application.Reporting;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace CLIPWEB.Application.Commands.Autocomplete;

/// <summary>Suggests editors by name; the option value is the editor profile id.</summary>
public class EditorAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var reporting = services.GetRequiredService<IReportingService>();
        var query = interaction.Data.Current.Value?.ToString();

        var editors = await reporting.SearchEditorsAsync(query);
        var results = editors.Select(e =>
            new AutocompleteResult(Truncate(e.Name), e.EditorProfileId.ToString()));

        return AutocompletionResult.FromSuccess(results);
    }

    private static string Truncate(string value)
        => value.Length <= 100 ? value : value[..97] + "...";
}
