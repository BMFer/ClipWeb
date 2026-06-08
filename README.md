# CLIPWEB

A Discord bot that tracks community-generated clips for brand campaigns run through clipping networks. See [`InitSpecs.md`](InitSpecs.md) for the full product specification.

> **Status:** 🎉 **MVP complete.** All six build phases are done. Editors are onboarded, campaigns are managed, clips are submitted and reviewed, published posts are tracked, and `/mystats` + `/report editor|campaign|brand` surface the numbers. See *Build order* below.

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
| `Roles:AdminRoleId`          | Role treated as Admin for manager commands (0 = off).|
| `Roles:NetworkManagerRoleId` | Role treated as Network Manager (0 = off).           |

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
- **`/campaigns`** – lists the active campaigns.
- **`/campaign details`** – shows a campaign (brand, dates, source, style guide).
- **`/brand create`** · **`/campaign create`** · **`/campaign close`** – manager
  commands; `create`/`close` use autocomplete to pick the brand/campaign.
- **`/submit clip`** – editors submit a clip (with URL validation) to an active
  campaign; creates the editor profile on first use.
- **`/submit post`** – editors log a published post (platform, URL, views, and
  optional likes/comments/shares) against one of their **own approved** clips
  (autocomplete is scoped to the caller's approved submissions).
- **`/mystats`** – an editor's own performance (clips, approval rate, posts,
  views, avg views/post, best post, active campaigns).
- **`/report editor`** · **`/report campaign`** · **`/report brand`** – manager
  reports aggregating clips, approvals, posts, views, and top performers.
- **`/leaderboard`** – top editors ranked by total views generated.

Reviewer decisions (`/submission approve|reject|revision`) persist the
reviewer's note, timestamp, and reviewer id on the submission. Duplicate clip
URLs (per campaign) and duplicate post URLs (globally) are rejected.
- **`/submission review`** – manager review queue of pending submissions.
- **`/submission approve|reject|revision`** – manager decisions (autocomplete
  the pending submission); the editor is DM'd the outcome and any reviewer note.

New members are also welcomed automatically on join (to `WelcomeChannelId`, or
by DM). Without a configured token the host still runs (applying migrations) but
stays offline.

### Manager commands & permissions

Brand/campaign management is gated by `[RequireManager]`: a user passes if they
have the Discord **Manage Server** permission **or** hold a configured
`Roles:AdminRoleId` / `Roles:NetworkManagerRoleId`. Failures reply with an
ephemeral "you need permission" message. (The check is enforced at runtime
rather than via Discord's hard command-visibility gate, so configured-role users
without Manage Server can still use the commands.)

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
3. **Campaigns** – brand & campaign creation, listing, details ✅ *done*
4. **Submissions** – submit, review, approve/reject/revision ✅ *done*
5. **Published posts** – post URLs, platform, view tracking ✅ *done*
6. **Reporting** – editor & campaign stats, admin reports ✅ *done*

See `InitSpecs.md` §12 for details.
