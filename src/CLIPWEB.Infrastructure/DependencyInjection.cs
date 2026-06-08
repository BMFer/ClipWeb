using CLIPWEB.Core.Interfaces;
using CLIPWEB.Infrastructure.Data;
using CLIPWEB.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CLIPWEB.Infrastructure;

/// <summary>
/// Registration helpers for the Infrastructure layer (database, repositories).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=clipweb.db";

        services.AddDbContext<ClipWebDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }
}
