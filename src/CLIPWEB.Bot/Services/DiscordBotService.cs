using CLIPWEB.Bot.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CLIPWEB.Bot.Services;

/// <summary>
/// Owns the Discord gateway connection lifecycle: logs in with the configured
/// bot token, starts the socket client, and disconnects cleanly on shutdown.
/// If no token is configured the bot stays offline (so the rest of the host —
/// e.g. database migrations — can still run during local development).
/// </summary>
public class DiscordBotService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordOptions _options;
    private readonly ILogger<DiscordBotService> _logger;

    public DiscordBotService(
        DiscordSocketClient client,
        IOptions<DiscordOptions> options,
        ILogger<DiscordBotService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += message => DiscordLogAdapter.LogAsync(_logger, message);

        if (string.IsNullOrWhiteSpace(_options.Token))
        {
            _logger.LogWarning(
                "No Discord token configured. The bot will not connect. " +
                "Set it via 'dotnet user-secrets set \"Discord:Token\" <token>' " +
                "or the Discord__Token environment variable.");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, _options.Token);
        await _client.StartAsync();
        _logger.LogInformation("Discord gateway client started.");

        // Stay alive until the host requests shutdown.
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown.
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Discord gateway client...");
        if (_client.LoginState == LoginState.LoggedIn)
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
