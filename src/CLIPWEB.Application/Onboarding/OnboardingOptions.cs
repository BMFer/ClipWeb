namespace CLIPWEB.Application.Onboarding;

/// <summary>
/// Server-specific onboarding settings, bound from the "Onboarding"
/// configuration section.
/// </summary>
public class OnboardingOptions
{
    public const string SectionName = "Onboarding";

    /// <summary>
    /// Role granted to an editor once they complete the survey.
    /// 0 = role assignment disabled.
    /// </summary>
    public ulong EditorRoleId { get; set; }

    /// <summary>
    /// Channel where the welcome message is posted when a member joins.
    /// 0 = fall back to a direct message.
    /// </summary>
    public ulong WelcomeChannelId { get; set; }
}
