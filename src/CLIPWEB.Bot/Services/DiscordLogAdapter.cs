using Discord;
using Microsoft.Extensions.Logging;

namespace CLIPWEB.Bot.Services;

/// <summary>
/// Forwards Discord.Net <see cref="LogMessage"/> events into the host's
/// <see cref="ILogger"/> pipeline (Serilog), mapping severity levels.
/// </summary>
internal static class DiscordLogAdapter
{
    public static Task LogAsync(ILogger logger, LogMessage message)
    {
        var level = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

        logger.Log(level, message.Exception, "[Discord:{Source}] {Message}",
            message.Source, message.Message);
        return Task.CompletedTask;
    }
}
