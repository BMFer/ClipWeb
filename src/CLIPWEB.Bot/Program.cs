using CLIPWEB.Application;
using CLIPWEB.Application.Onboarding;
using CLIPWEB.Bot.Configuration;
using CLIPWEB.Bot.Services;
using CLIPWEB.Infrastructure;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Structured logging via Serilog, configured from appsettings.
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Bind Discord settings (token supplied via user-secrets / env vars).
builder.Services.Configure<DiscordOptions>(
    builder.Configuration.GetSection(DiscordOptions.SectionName));

// Bind onboarding settings (editor role, welcome channel).
builder.Services.Configure<OnboardingOptions>(
    builder.Configuration.GetSection(OnboardingOptions.SectionName));

// Data + application layers.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Ensure the database schema exists before anything else runs.
builder.Services.AddHostedService<DatabaseInitializer>();

// Discord gateway client + interaction (slash command) service.
builder.Services.AddSingleton(new DiscordSocketConfig
{
    // GuildMembers is privileged — enable it in the Discord developer portal.
    // Needed for the welcome-on-join handler and for resolving guild users
    // when assigning the editor role.
    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers,
    LogLevel = LogSeverity.Info,
    AlwaysDownloadUsers = true
});
builder.Services.AddSingleton(sp =>
    new DiscordSocketClient(sp.GetRequiredService<DiscordSocketConfig>()));
builder.Services.AddSingleton(sp => new InteractionService(
    sp.GetRequiredService<DiscordSocketClient>(),
    new InteractionServiceConfig { LogLevel = LogSeverity.Info, UseCompiledLambda = true }));

// Register modules + dispatch interactions, post welcomes on join, then connect.
// Order matters: handlers subscribe to gateway events before the bot logs in.
builder.Services.AddHostedService<InteractionHandler>();
builder.Services.AddHostedService<GuildMemberHandler>();
builder.Services.AddHostedService<DiscordBotService>();

var host = builder.Build();
host.Run();
