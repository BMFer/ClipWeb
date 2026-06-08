using CLIPWEB.Application.Onboarding;
using CLIPWEB.Core.Entities;
using CLIPWEB.Infrastructure.Data;
using CLIPWEB.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CLIPWEB.Tests;

public class EditorOnboardingServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public EditorOnboardingServiceTests()
    {
        // A single open in-memory connection keeps the schema/data alive across
        // the many short-lived contexts created below.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        using var seed = CreateContext();
        seed.Database.EnsureCreated();
    }

    private ClipWebDbContext CreateContext()
        => new(new DbContextOptionsBuilder<ClipWebDbContext>().UseSqlite(_connection).Options);

    // A fresh service over a fresh context, mirroring the per-interaction scope
    // the bot uses at runtime.
    private EditorOnboardingService NewService(out ClipWebDbContext context)
    {
        context = CreateContext();
        return new EditorOnboardingService(new EfRepository<EditorProfile>(context));
    }

    private async Task<int> ProfileCountAsync()
    {
        using var ctx = CreateContext();
        return await ctx.EditorProfiles.CountAsync();
    }

    [Fact]
    public async Task GetProfile_returns_null_when_editor_is_unknown()
    {
        var service = NewService(out var ctx);
        using (ctx)
            Assert.Null(await service.GetProfileAsync(42UL));
    }

    [Fact]
    public async Task GetOrCreate_creates_once_and_is_idempotent()
    {
        Guid firstId;
        using (var ctx = CreateContext())
        {
            var svc = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
            firstId = (await svc.GetOrCreateProfileAsync(123UL, "clipper")).Id;
        }

        using (var ctx = CreateContext())
        {
            var svc = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
            var second = await svc.GetOrCreateProfileAsync(123UL, "clipper-renamed");
            Assert.Equal(firstId, second.Id);
            Assert.Equal("clipper-renamed", second.DiscordUsername); // username kept in sync
            Assert.False(second.SurveyCompleted);
        }

        Assert.Equal(1, await ProfileCountAsync());
    }

    [Fact]
    public async Task Survey_flow_persists_all_answers_and_marks_complete()
    {
        const ulong userId = 999UL;

        using (var ctx = CreateContext())
        {
            var svc = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
            await svc.SaveSurveyPart1Async(userId, "clipper",
                new SurveyPart1("Jamie", "EST", "CapCut", "TikTok, Reels", "Advanced"));
        }

        // After part 1 the profile exists but is not yet complete.
        using (var ctx = CreateContext())
        {
            var svc = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
            var midway = await svc.GetProfileAsync(userId);
            Assert.NotNull(midway);
            Assert.False(midway!.SurveyCompleted);
            Assert.Equal("Jamie", midway.PreferredName);
        }

        using (var ctx = CreateContext())
        {
            var svc = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
            await svc.CompleteSurveyAsync(userId, "clipper",
                new SurveyPart2("gaming, comedy", 12, "Yes", "https://port.folio", "Discord DM"));
        }

        using (var ctx = CreateContext())
        {
            var svc = new EditorOnboardingService(new EfRepository<EditorProfile>(ctx));
            var final = await svc.GetProfileAsync(userId);
            Assert.NotNull(final);
            Assert.Multiple(
                () => Assert.True(final!.SurveyCompleted),
                () => Assert.NotNull(final!.SurveyCompletedAtUtc),
                () => Assert.Equal("EST", final!.TimeZone),
                () => Assert.Equal("CapCut", final!.EditingSoftware),
                () => Assert.Equal("TikTok, Reels", final!.PrimaryPlatform),
                () => Assert.Equal("Advanced", final!.ExperienceLevel),
                () => Assert.Equal("gaming, comedy", final!.ContentNiche),
                () => Assert.Equal(12, final!.ClipsPerWeek),
                () => Assert.Equal("Yes", final!.CanSelfPublish),
                () => Assert.Equal("https://port.folio", final!.PortfolioUrl),
                () => Assert.Equal("Discord DM", final!.ContactPreference));
        }

        // Still exactly one row — the flow updates, never duplicates.
        Assert.Equal(1, await ProfileCountAsync());
    }

    public void Dispose() => _connection.Dispose();
}
