using CLIPWEB.Core.Entities;
using CLIPWEB.Core.Interfaces;

namespace CLIPWEB.Application.Onboarding;

/// <inheritdoc />
public class EditorOnboardingService : IEditorOnboardingService
{
    private readonly IRepository<EditorProfile> _profiles;

    public EditorOnboardingService(IRepository<EditorProfile> profiles)
    {
        _profiles = profiles;
    }

    public async Task<EditorProfile?> GetProfileAsync(ulong discordUserId, CancellationToken ct = default)
    {
        var matches = await _profiles.ListAsync(p => p.DiscordUserId == discordUserId, ct);
        return matches.FirstOrDefault();
    }

    public Task<EditorProfile> GetOrCreateProfileAsync(
        ulong discordUserId, string discordUsername, CancellationToken ct = default)
        => UpsertAsync(discordUserId, discordUsername, _ => { }, ct);

    public Task SaveSurveyPart1Async(
        ulong discordUserId, string discordUsername, SurveyPart1 answers, CancellationToken ct = default)
        => UpsertAsync(discordUserId, discordUsername, profile =>
        {
            profile.PreferredName = answers.PreferredName;
            profile.TimeZone = answers.TimeZone;
            profile.EditingSoftware = answers.EditingSoftware;
            profile.PrimaryPlatform = answers.PostingPlatforms;
            profile.ExperienceLevel = answers.ExperienceLevel;
        }, ct);

    public Task<EditorProfile> CompleteSurveyAsync(
        ulong discordUserId, string discordUsername, SurveyPart2 answers, CancellationToken ct = default)
        => UpsertAsync(discordUserId, discordUsername, profile =>
        {
            profile.ContentNiche = answers.ContentNiche;
            profile.ClipsPerWeek = answers.ClipsPerWeek;
            profile.CanSelfPublish = answers.CanSelfPublish;
            profile.PortfolioUrl = answers.PortfolioUrl;
            profile.ContactPreference = answers.ContactPreference;
            profile.SurveyCompleted = true;
            profile.SurveyCompletedAtUtc = DateTime.UtcNow;
        }, ct);

    /// <summary>
    /// Loads the profile for a Discord user (or creates a blank one), keeps the
    /// username in sync, applies <paramref name="mutate"/>, and persists.
    /// </summary>
    private async Task<EditorProfile> UpsertAsync(
        ulong discordUserId, string discordUsername, Action<EditorProfile> mutate, CancellationToken ct)
    {
        var existing = (await _profiles.ListAsync(p => p.DiscordUserId == discordUserId, ct))
            .FirstOrDefault();

        if (existing is null)
        {
            var profile = new EditorProfile
            {
                Id = Guid.NewGuid(),
                DiscordUserId = discordUserId,
                DiscordUsername = discordUsername,
                CreatedAtUtc = DateTime.UtcNow
            };
            mutate(profile);
            await _profiles.AddAsync(profile, ct);
            await _profiles.SaveChangesAsync(ct);
            return profile;
        }

        existing.DiscordUsername = discordUsername;
        mutate(existing);
        _profiles.Update(existing);
        await _profiles.SaveChangesAsync(ct);
        return existing;
    }
}
