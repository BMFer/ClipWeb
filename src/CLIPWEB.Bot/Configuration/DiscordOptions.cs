namespace CLIPWEB.Bot.Configuration;

/// <summary>
/// Strongly-typed Discord settings bound from the "Discord" configuration section.
/// The bot token should come from user-secrets or an environment variable,
/// never from a committed appsettings file.
/// </summary>
public class DiscordOptions
{
    public const string SectionName = "Discord";

    /// <summary>Bot token. Set via user-secrets / env var (Discord__Token).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Optional guild id for fast (guild-scoped) slash command registration
    /// during development. 0 = register globally.
    /// </summary>
    public ulong DevGuildId { get; set; }
}
