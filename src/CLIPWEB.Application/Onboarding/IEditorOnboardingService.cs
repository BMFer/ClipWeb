using CLIPWEB.Core.Entities;

namespace CLIPWEB.Application.Onboarding;

/// <summary>
/// Manages editor profiles and the onboarding survey lifecycle.
/// </summary>
public interface IEditorOnboardingService
{
    /// <summary>Returns the editor's profile, or null if they have none yet.</summary>
    Task<EditorProfile?> GetProfileAsync(ulong discordUserId, CancellationToken ct = default);

    /// <summary>
    /// Returns the editor's profile, creating a blank one if it does not exist.
    /// Keeps the stored Discord username in sync.
    /// </summary>
    Task<EditorProfile> GetOrCreateProfileAsync(
        ulong discordUserId, string discordUsername, CancellationToken ct = default);

    /// <summary>Saves the first half of the survey (creating the profile if needed).</summary>
    Task SaveSurveyPart1Async(
        ulong discordUserId, string discordUsername, SurveyPart1 answers, CancellationToken ct = default);

    /// <summary>
    /// Saves the second half of the survey, marks the survey complete, and
    /// returns the finished profile.
    /// </summary>
    Task<EditorProfile> CompleteSurveyAsync(
        ulong discordUserId, string discordUsername, SurveyPart2 answers, CancellationToken ct = default);
}
