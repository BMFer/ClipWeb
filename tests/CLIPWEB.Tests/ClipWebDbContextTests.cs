using CLIPWEB.Core.Entities;
using CLIPWEB.Core.Enums;
using CLIPWEB.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CLIPWEB.Tests;

public class ClipWebDbContextTests
{
    private static ClipWebDbContext CreateInMemoryContext(out SqliteConnection connection)
    {
        // A shared, open in-memory SQLite connection lives for the test's duration.
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ClipWebDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ClipWebDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task Can_persist_and_read_back_a_campaign_with_a_submission()
    {
        using var context = CreateInMemoryContext(out var connection);
        try
        {
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Acme", CreatedAtUtc = DateTime.UtcNow };
            var campaign = new Campaign
            {
                Id = Guid.NewGuid(),
                BrandId = brand.Id,
                Name = "Launch Week",
                Description = "Clip the keynote",
                StartDateUtc = DateTime.UtcNow,
                IsActive = true
            };
            var editor = new EditorProfile
            {
                Id = Guid.NewGuid(),
                DiscordUserId = 1234567890UL,
                DiscordUsername = "clipper",
                CreatedAtUtc = DateTime.UtcNow
            };
            var submission = new ClipSubmission
            {
                Id = Guid.NewGuid(),
                CampaignId = campaign.Id,
                EditorProfileId = editor.Id,
                ClipUrl = "https://example.com/clip",
                Status = SubmissionStatus.Pending,
                SubmittedAtUtc = DateTime.UtcNow
            };

            context.AddRange(brand, campaign, editor, submission);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            var loaded = await context.Campaigns
                .Include(c => c.Submissions)
                .SingleAsync(c => c.Id == campaign.Id);

            Assert.Equal("Launch Week", loaded.Name);
            Assert.Single(loaded.Submissions);
            Assert.Equal(SubmissionStatus.Pending, loaded.Submissions.First().Status);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task SubmissionStatus_is_stored_as_a_string()
    {
        using var context = CreateInMemoryContext(out var connection);
        try
        {
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Acme", CreatedAtUtc = DateTime.UtcNow };
            var campaign = new Campaign
            {
                Id = Guid.NewGuid(),
                BrandId = brand.Id,
                Name = "Launch Week",
                Description = "Clip the keynote",
                StartDateUtc = DateTime.UtcNow,
                IsActive = true
            };
            var editor = new EditorProfile
            {
                Id = Guid.NewGuid(),
                DiscordUserId = 1234567890UL,
                DiscordUsername = "clipper",
                CreatedAtUtc = DateTime.UtcNow
            };
            var submission = new ClipSubmission
            {
                Id = Guid.NewGuid(),
                CampaignId = campaign.Id,
                EditorProfileId = editor.Id,
                ClipUrl = "https://example.com/clip",
                Status = SubmissionStatus.Approved,
                SubmittedAtUtc = DateTime.UtcNow
            };
            context.AddRange(brand, campaign, editor, submission);
            await context.SaveChangesAsync();

            // Only one submission exists, so read its raw stored Status value.
            var raw = await connection.CreateCommand()
                .ExecuteScalarStringAsync("SELECT Status FROM ClipSubmissions LIMIT 1");

            Assert.Equal("Approved", raw);
        }
        finally
        {
            connection.Dispose();
        }
    }
}

internal static class SqliteTestExtensions
{
    public static async Task<string?> ExecuteScalarStringAsync(this SqliteCommand command, string sql)
    {
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync();
        return result as string;
    }
}
