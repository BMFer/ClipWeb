using CLIPWEB.Application.Commands.Modals;
using CLIPWEB.Application.Onboarding;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CLIPWEB.Application.Commands;

/// <summary>
/// The <c>/survey</c> onboarding flow: a two-step modal form that captures the
/// editor's profile and grants the editor role on completion.
/// </summary>
public class SurveyModule : InteractionModuleBase<SocketInteractionContext>
{
    // Component / modal custom ids.
    private const string StartButtonId = "survey_start";
    private const string ContinueButtonId = "survey_continue";
    private const string Part1ModalId = "survey_part1";
    private const string Part2ModalId = "survey_part2";

    private readonly IEditorOnboardingService _onboarding;
    private readonly OnboardingOptions _options;
    private readonly ILogger<SurveyModule> _logger;

    public SurveyModule(
        IEditorOnboardingService onboarding,
        IOptions<OnboardingOptions> options,
        ILogger<SurveyModule> logger)
    {
        _onboarding = onboarding;
        _options = options.Value;
        _logger = logger;
    }

    [SlashCommand("survey", "Start the CLIPWEB editor onboarding survey.")]
    public async Task StartSurveyAsync()
    {
        var existing = await _onboarding.GetProfileAsync(Context.User.Id);
        var note = existing?.SurveyCompleted == true
            ? "You've completed the survey before — running it again will update your profile.\n\n"
            : string.Empty;

        var components = new ComponentBuilder()
            .WithButton("Start Survey", StartButtonId, ButtonStyle.Primary, new Emoji("📝"))
            .Build();

        await RespondAsync(
            $"{note}This quick survey has **two short steps**. Your answers build your editor profile inside the network.",
            components: components,
            ephemeral: true);
    }

    [ComponentInteraction(StartButtonId, ignoreGroupNames: true)]
    public async Task ShowPart1Async()
        => await RespondWithModalAsync<SurveyPart1Modal>(Part1ModalId);

    [ModalInteraction(Part1ModalId, ignoreGroupNames: true)]
    public async Task SubmitPart1Async(SurveyPart1Modal modal)
    {
        var answers = new SurveyPart1(
            Clean(modal.PreferredName),
            Clean(modal.TimeZone),
            Clean(modal.EditingSoftware),
            Clean(modal.PostingPlatforms),
            Clean(modal.ExperienceLevel));

        await _onboarding.SaveSurveyPart1Async(Context.User.Id, Context.User.Username, answers);

        var components = new ComponentBuilder()
            .WithButton("Continue", ContinueButtonId, ButtonStyle.Primary, new Emoji("➡️"))
            .Build();

        await RespondAsync(
            "✅ Step 1 saved. Tap **Continue** to finish the last few questions.",
            components: components,
            ephemeral: true);
    }

    [ComponentInteraction(ContinueButtonId, ignoreGroupNames: true)]
    public async Task ShowPart2Async()
        => await RespondWithModalAsync<SurveyPart2Modal>(Part2ModalId);

    [ModalInteraction(Part2ModalId, ignoreGroupNames: true)]
    public async Task SubmitPart2Async(SurveyPart2Modal modal)
    {
        var answers = new SurveyPart2(
            Clean(modal.ContentNiche),
            ParseClips(modal.ClipsPerWeek),
            Clean(modal.CanSelfPublish),
            Clean(modal.PortfolioUrl),
            Clean(modal.ContactPreference));

        await _onboarding.CompleteSurveyAsync(Context.User.Id, Context.User.Username, answers);
        var roleNote = await TryAssignEditorRoleAsync();

        await RespondAsync(
            $"🎉 You're all set! Your editor profile is complete.{roleNote}\nUse **/campaigns** to see what's active.",
            ephemeral: true);
    }

    /// <summary>Grants the configured editor role; returns a user-facing note.</summary>
    private async Task<string> TryAssignEditorRoleAsync()
    {
        if (_options.EditorRoleId == 0 || Context.Guild is null)
            return string.Empty;

        var role = Context.Guild.GetRole(_options.EditorRoleId);
        if (role is null)
        {
            _logger.LogWarning(
                "Configured EditorRoleId {RoleId} was not found in guild {GuildId}.",
                _options.EditorRoleId, Context.Guild.Id);
            return string.Empty;
        }

        var guildUser = Context.Guild.GetUser(Context.User.Id);
        if (guildUser is null)
            return string.Empty;

        if (guildUser.Roles.Any(r => r.Id == role.Id))
            return $" You already have the **{role.Name}** role.";

        try
        {
            await guildUser.AddRoleAsync(role);
            return $" You've been given the **{role.Name}** role.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to assign role {RoleId} to user {UserId} in guild {GuildId}.",
                role.Id, Context.User.Id, Context.Guild.Id);
            return string.Empty;
        }
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParseClips(string? value)
        => int.TryParse(Clean(value), out var n) && n >= 0 ? n : null;
}
