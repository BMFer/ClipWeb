using CLIPWEB.Application.Campaigns;
using CLIPWEB.Application.Onboarding;
using CLIPWEB.Application.Submissions;
using Microsoft.Extensions.DependencyInjection;

namespace CLIPWEB.Application;

/// <summary>
/// Registration helpers for the Application layer (commands, surveys, reports).
/// Command modules and handlers will be registered here as they are built out
/// (see MVP build order in InitSpecs.md).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Onboarding (Phase 2).
        services.AddScoped<IEditorOnboardingService, EditorOnboardingService>();

        // Brands & campaigns (Phase 3).
        services.AddScoped<ICampaignService, CampaignService>();

        // Submissions (Phase 4).
        services.AddScoped<ISubmissionService, SubmissionService>();

        // Published posts (Phase 5).
        services.AddScoped<IPublishedPostService, PublishedPostService>();

        return services;
    }
}
