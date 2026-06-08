using Discord;
using Discord.Interactions;

namespace CLIPWEB.Application.Commands.Modals;

/// <summary>Onboarding survey, second half (questions 6-10).</summary>
public class SurveyPart2Modal : IModal
{
    public string Title => "CLIPWEB Survey (2 of 2)";

    [InputLabel("What content do you clip best?")]
    [ModalTextInput("content_niche", TextInputStyle.Paragraph, "podcasts, gaming, business, crypto, fitness...", maxLength: 500)]
    [RequiredInput(false)]
    public string? ContentNiche { get; set; }

    [InputLabel("How many clips can you make per week?")]
    [ModalTextInput("clips_per_week", TextInputStyle.Short, "A number, e.g. 10", maxLength: 10)]
    [RequiredInput(false)]
    public string? ClipsPerWeek { get; set; }

    [InputLabel("Can you publish on your own accounts?")]
    [ModalTextInput("can_self_publish", TextInputStyle.Short, "Yes / No / Sometimes", maxLength: 20)]
    [RequiredInput(false)]
    public string? CanSelfPublish { get; set; }

    [InputLabel("Drop a portfolio link or example post.")]
    [ModalTextInput("portfolio_url", TextInputStyle.Short, "https://...", maxLength: 500)]
    [RequiredInput(false)]
    public string? PortfolioUrl { get; set; }

    [InputLabel("Best way for managers to contact you?")]
    [ModalTextInput("contact_preference", TextInputStyle.Short, "Discord DM, email, etc.", maxLength: 200)]
    [RequiredInput(false)]
    public string? ContactPreference { get; set; }
}
