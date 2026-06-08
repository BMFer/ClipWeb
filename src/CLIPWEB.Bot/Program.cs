using CLIPWEB.Application;
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

// Data + application layers.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Ensure the database schema exists before anything else runs.
builder.Services.AddHostedService<DatabaseInitializer>();

// Discord gateway client + interaction (slash command) service.
builder.Services.AddSingleton(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds,
    LogLevel = LogSeverity.Info,
    AlwaysDownloadUsers = false
});
builder.Services.AddSingleton(sp =>
    new DiscordSocketClient(sp.GetRequiredService<DiscordSocketConfig>()));
builder.Services.AddSingleton(sp => new InteractionService(
    sp.GetRequiredService<DiscordSocketClient>(),
    new InteractionServiceConfig { LogLevel = LogSeverity.Info, UseCompiledLambda = true }));

// Register modules + dispatch interactions, then connect the gateway.
// Order matters: the handler subscribes to gateway events before the bot logs in.
builder.Services.AddHostedService<InteractionHandler>();
builder.Services.AddHostedService<DiscordBotService>();

var host = builder.Build();
host.Run();
