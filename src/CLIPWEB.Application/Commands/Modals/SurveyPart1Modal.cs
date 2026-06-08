using Discord;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands.Modals;

/// <summary>Onboarding survey, first half (questions 1-5).</summary>
public class SurveyPart1Modal : IModal
{
    public string Title => "CLIPWEB Survey (1 of 2)";

    [InputLabel("What name should we call you?")]
    [ModalTextInput("preferred_name", TextInputStyle.Short, "Your preferred name", maxLength: 100)]
    public string? PreferredName { get; set; }

    [InputLabel("What time zone are you in?")]
    [ModalTextInput("timezone", TextInputStyle.Short, "e.g. EST, UTC+1, PST", maxLength: 100)]
    [RequiredInput(false)]
    public string? TimeZone { get; set; }

    [InputLabel("What do you edit with?")]
    [ModalTextInput("editing_software", TextInputStyle.Short, "CapCut, Premiere Pro, DaVinci, Final Cut...", maxLength: 100)]
    [RequiredInput(false)]
    public string? EditingSoftware { get; set; }

    [InputLabel("What platforms do you mainly post on?")]
    [ModalTextInput("posting_platforms", TextInputStyle.Short, "TikTok, YouTube Shorts, Reels, X...", maxLength: 200)]
    [RequiredInput(false)]
    public string? PostingPlatforms { get; set; }

    [InputLabel("What is your experience level?")]
    [ModalTextInput("experience_level", TextInputStyle.Short, "Beginner, Intermediate, Advanced, Professional", maxLength: 50)]
    [RequiredInput(false)]
    public string? ExperienceLevel { get; set; }
}
