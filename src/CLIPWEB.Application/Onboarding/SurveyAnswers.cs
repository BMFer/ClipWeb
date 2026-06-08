namespace CLIPWEB.Application.Onboarding;

/// <summary>First half of the onboarding survey (questions 1-5).</summary>
public record SurveyPart1(
    string? PreferredName,
    string? TimeZone,
    string? EditingSoftware,
    string? PostingPlatforms,
    string? ExperienceLevel);

/// <summary>Second half of the onboarding survey (questions 6-10).</summary>
public record SurveyPart2(
    string? ContentNiche,
    int? ClipsPerWeek,
    string? CanSelfPublish,
    string? PortfolioUrl,
    string? ContactPreference);
