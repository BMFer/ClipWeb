using CLIPWEB.Application.Submissions;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace CLIPWEB.Application.Commands.Autocomplete;

/// <summary>Suggests pending submissions; value is the submission id.</summary>
public class PendingSubmissionAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var service = services.GetRequiredService<ISubmissionService>();
        var query = interaction.Data.Current.Value?.ToString();

        var pending = await service.SearchPendingAsync(query);
        var results = pending.Select(s =>
            new AutocompleteResult(Label(s), s.SubmissionId.ToString()));

        return AutocompletionResult.FromSuccess(results);
    }

    private static string Label(SubmissionSummary s)
    {
        var label = $"{s.CampaignName} · {s.EditorName} · {s.SubmittedAtUtc:yyyy-MM-dd}";
        return label.Length <= 100 ? label : label[..97] + "...";
    }
}
