using CLIPWEB.Application;
using CLIPWEB.Bot.Configuration;
using CLIPWEB.Bot.Services;
using CLIPWEB.Infrastructure;
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

// TODO (Phase 1): register the Discord client + interaction service and the
// hosted service that connects the bot and dispatches slash commands.

var host = builder.Build();
host.Run();
