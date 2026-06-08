# CLIPWEB

A Discord bot that tracks community-generated clips for brand campaigns run through clipping networks. See [`InitSpecs.md`](InitSpecs.md) for the full product specification.

> **Status:** Phase 2 (onboarding) complete. On top of the Phase 1 foundation, CLIPWEB now has `/welcome`, a two-step `/survey` flow that builds the editor profile, welcome-on-join, and post-survey editor-role assignment. Campaign features come next — see *Build order* below.

## Tech stack

- **.NET 10** / C#
- **Discord.Net** (referenced; interaction handling pending)
- **Entity Framework Core 10** with **SQLite** for local development
- **Serilog** for structured logging
- **xUnit** for tests

## Solution layout

```
CLIPWEB.slnx
src/
  CLIPWEB.Core           Domain entities, enums, and abstractions (no dependencies)
  CLIPWEB.Infrastructure EF Core DbContext, entity configs, repositories, DI wiring
  CLIPWEB.Application     Command/survey/report/workflow services (registration stub)
  CLIPWEB.Bot            Generic Host entry point, configuration, DB initializer
tests/
  CLIPWEB.Tests          xUnit tests
```

The dependency direction is `Bot → Application/Infrastructure → Core`. Core has no
project dependencies.

## Getting started

```powershell
# Restore & build
dotnet build

# Run the tests
dotnet test

# Run the bot (applies migrations to ./clipweb.db on startup)
dotnet run --project src/CLIPWEB.Bot
```

### Configuration

Settings live in `src/CLIPWEB.Bot/appsettings.json`. The Discord bot token must **not**
be committed — supply it via user-secrets or an environment variable:

```powershell
# Option A: user-secrets (development)
dotnet user-secrets --project src/CLIPWEB.Bot set "Discord:Token" "<your-token>"

# Option B: environment variable
$env:Discord__Token = "<your-token>"
```

| Key                          | Purpose                                              |
| ---------------------------- | ---------------------------------------------------- |
| `ConnectionStrings:Default`  | SQLite connection string (default `clipweb.db`).     |
| `Discord:Token`              | Bot token (set via secrets/env, never committed).    |
| `Discord:DevGuildId`         | Guild id for fast dev command registration (0=global)|
| `Onboarding:EditorRoleId`    | Role granted on survey completion (0 = disabled).    |
| `Onboarding:WelcomeChannelId`| Channel for welcome-on-join (0 = DM the new member). |

> **Privileged intent:** welcome-on-join and editor-role assignment use the
> **Server Members Intent**. Enable it for your app under
> *Discord Developer Portal → Bot → Privileged Gateway Intents*.

## Slash commands

Command modules live in `CLIPWEB.Application/Commands` (Discord.Net
`InteractionModuleBase`). They are discovered automatically and registered with
Discord when the gateway connects:

- `Discord:DevGuildId` set to a guild id → commands register to that guild
  instantly (best for development).
- `Discord:DevGuildId = 0` → commands register globally (can take up to an hour
  to propagate).

Current commands:

- **`/ping`** – diagnostic; reports gateway latency.
- **`/welcome`** – posts the official CLIPWEB welcome message.
- **`/survey`** – two-step modal onboarding form (10 questions) that builds the
  editor's profile and grants the editor role on completion.

New members are also welcomed automatically on join (to `WelcomeChannelId`, or
by DM). Without a configured token the host still runs (applying migrations) but
stays offline.

## Database & migrations

The schema is managed with EF Core migrations (in
`src/CLIPWEB.Infrastructure/Data/Migrations`). On startup the bot applies any pending
migrations automatically via `DatabaseInitializer`.

```powershell
# Add a new migration after changing entities/configurations
dotnet ef migrations add <Name> `
  --project src/CLIPWEB.Infrastructure `
  --startup-project src/CLIPWEB.Bot `
  --output-dir Data/Migrations

# Apply migrations manually (optional; the bot also does this on startup)
dotnet ef database update `
  --project src/CLIPWEB.Infrastructure `
  --startup-project src/CLIPWEB.Bot
```

## Build order (from the spec)

1. **Foundation** – Discord client, slash commands, config, database, logging ✅ *done*
2. **Onboarding** – welcome message, survey, editor profiles ✅ *done*
3. **Campaigns** – brand & campaign creation, listing, details
4. **Submissions** – submit, review, approve/reject/revision
5. **Published posts** – post URLs, platform, view tracking
6. **Reporting** – editor & campaign stats, admin reports

See `InitSpecs.md` §12 for details.
