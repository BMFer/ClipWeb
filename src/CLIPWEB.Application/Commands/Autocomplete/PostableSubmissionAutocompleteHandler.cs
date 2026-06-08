using CLIPWEB.Application.Submissions;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace CLIPWEB.Application.Commands.Autocomplete;

/// <summary>
/// Suggests the current editor's own approved submissions (for /submit post);
/// the option value is the submission id.
/// </summary>
public class PostableSubmissionAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction interaction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var service = services.GetRequiredService<ISubmissionService>();
        var query = interaction.Data.Current.Value?.ToString();

        var approved = await service.GetApprovedForEditorAsync(context.User.Id, query);
        var results = approved.Select(s =>
            new AutocompleteResult(Label(s), s.SubmissionId.ToString()));

        return AutocompletionResult.FromSuccess(results);
    }

    private static string Label(SubmissionSummary s)
    {
        var label = $"{s.CampaignName} · {s.SubmittedAtUtc:yyyy-MM-dd}";
        return label.Length <= 100 ? label : label[..97] + "...";
    }
}
