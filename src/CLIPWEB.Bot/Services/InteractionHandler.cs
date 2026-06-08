using CLIPWEB.Bot.Configuration;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CLIPWEB.Bot.Services;

/// <summary>
/// Loads the application's slash command modules, registers them with Discord
/// once the gateway is ready, and dispatches incoming interactions to the
/// <see cref="InteractionService"/>.
/// </summary>
public class InteractionHandler : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly DiscordOptions _options;
    private readonly ILogger<InteractionHandler> _logger;

    private bool _commandsRegistered;

    public InteractionHandler(
        DiscordSocketClient client,
        InteractionService interactions,
        IServiceProvider services,
        IOptions<DiscordOptions> options,
        ILogger<InteractionHandler> logger)
    {
        _client = client;
        _interactions = interactions;
        _services = services;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Discover command modules from the Application assembly.
        await _interactions.AddModulesAsync(
            typeof(CLIPWEB.Application.Commands.PingModule).Assembly, _services);

        _interactions.Log += message => DiscordLogAdapter.LogAsync(_logger, message);
        _interactions.InteractionExecuted += OnInteractionExecutedAsync;

        _client.Ready += OnReadyAsync;
        _client.InteractionCreated += OnInteractionCreatedAsync;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Ready -= OnReadyAsync;
        _client.InteractionCreated -= OnInteractionCreatedAsync;
        _interactions.InteractionExecuted -= OnInteractionExecutedAsync;
        return Task.CompletedTask;
    }

    private async Task OnReadyAsync()
    {
        // Ready can fire again after reconnects; only register commands once.
        if (_commandsRegistered)
            return;
        _commandsRegistered = true;

        try
        {
            if (_options.DevGuildId != 0)
            {
                await _interactions.RegisterCommandsToGuildAsync(_options.DevGuildId);
                _logger.LogInformation(
                    "Registered {Count} slash command(s) to dev guild {GuildId}.",
                    _interactions.SlashCommands.Count, _options.DevGuildId);
            }
            else
            {
                await _interactions.RegisterCommandsGloballyAsync();
                _logger.LogInformation(
                    "Registered {Count} slash command(s) globally (can take up to an hour to appear).",
                    _interactions.SlashCommands.Count);
            }
        }
        catch (Exception ex)
        {
            _commandsRegistered = false;
            _logger.LogError(ex, "Failed to register slash commands.");
        }
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        try
        {
            // A fresh DI scope per interaction so command modules can resolve
            // scoped services (e.g. repositories / DbContext).
            using var scope = _services.CreateScope();
            var context = new SocketInteractionContext(_client, interaction);
            var result = await _interactions.ExecuteCommandAsync(context, scope.ServiceProvider);

            // A failed precondition (e.g. not a manager) never runs the command
            // body, so surface the reason to the user ourselves.
            if (!result.IsSuccess &&
                result.Error == InteractionCommandError.UnmetPrecondition &&
                !interaction.HasResponded)
            {
                await interaction.RespondAsync(result.ErrorReason, ephemeral: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while processing interaction {Id}.", interaction.Id);

            // If we already acknowledged, delete the original response to avoid a
            // dangling "thinking..." state.
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                var response = await interaction.GetOriginalResponseAsync();
                if (response is not null)
                    await response.DeleteAsync();
            }
        }
    }

    private Task OnInteractionExecutedAsync(
        ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess && result.Error != InteractionCommandError.UnknownCommand)
        {
            _logger.LogWarning(
                "Interaction '{Command}' failed: {Error} - {Reason}",
                command?.Name, result.Error, result.ErrorReason);
        }
        return Task.CompletedTask;
    }
}
