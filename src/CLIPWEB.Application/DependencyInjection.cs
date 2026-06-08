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
        // Placeholder: application services (survey flow, reporting, submission
        // workflow) are registered here in later phases.
        return services;
    }
}
