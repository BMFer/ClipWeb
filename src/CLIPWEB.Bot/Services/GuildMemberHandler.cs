using CLIPWEB.Application.Onboarding;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CLIPWEB.Bot.Services;

/// <summary>
/// Posts the CLIPWEB welcome message when a new member joins a guild — to the
/// configured welcome channel if set, otherwise via direct message.
/// Requires the privileged <c>GuildMembers</c> gateway intent.
/// </summary>
public class GuildMemberHandler : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly OnboardingOptions _options;
    private readonly ILogger<GuildMemberHandler> _logger;

    public GuildMemberHandler(
        DiscordSocketClient client,
        IOptions<OnboardingOptions> options,
        ILogger<GuildMemberHandler> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _client.UserJoined += OnUserJoinedAsync;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.UserJoined -= OnUserJoinedAsync;
        return Task.CompletedTask;
    }

    private async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        if (user.IsBot)
            return;

        var embed = WelcomeMessage.BuildEmbed();

        try
        {
            if (_options.WelcomeChannelId != 0)
            {
                var channel = user.Guild.GetTextChannel(_options.WelcomeChannelId);
                if (channel is not null)
                {
                    await channel.SendMessageAsync(text: user.Mention, embed: embed);
                    return;
                }

                _logger.LogWarning(
                    "Welcome channel {ChannelId} not found in guild {GuildId}; falling back to DM.",
                    _options.WelcomeChannelId, user.Guild.Id);
            }

            var dm = await user.CreateDMChannelAsync();
            await dm.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            // DMs may be closed, or the bot may lack channel permissions.
            _logger.LogWarning(ex,
                "Could not deliver the welcome message to user {UserId} in guild {GuildId}.",
                user.Id, user.Guild.Id);
        }
    }
}
