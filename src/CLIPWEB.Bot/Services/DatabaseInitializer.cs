using CLIPWEB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CLIPWEB.Bot.Services;

/// <summary>
/// Applies any pending EF Core migrations on startup so the SQLite database
/// is ready before the bot begins handling commands.
/// </summary>
public class DatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IServiceProvider services, ILogger<DatabaseInitializer> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Applying database migrations...");
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClipWebDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Database is up to date.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
