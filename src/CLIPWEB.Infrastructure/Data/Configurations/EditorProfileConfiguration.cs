using CLIPWEB.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CLIPWEB.Infrastructure.Data.Configurations;

public class EditorProfileConfiguration : IEntityTypeConfiguration<EditorProfile>
{
    public void Configure(EntityTypeBuilder<EditorProfile> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DiscordUsername).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PreferredName).HasMaxLength(100);
        builder.Property(e => e.TimeZone).HasMaxLength(100);
        builder.Property(e => e.EditingSoftware).HasMaxLength(100);
        builder.Property(e => e.PrimaryPlatform).HasMaxLength(200);
        builder.Property(e => e.ExperienceLevel).HasMaxLength(50);
        builder.Property(e => e.ContentNiche).HasMaxLength(500);
        builder.Property(e => e.CanSelfPublish).HasMaxLength(20);
        builder.Property(e => e.PortfolioUrl).HasMaxLength(500);
        builder.Property(e => e.ContactPreference).HasMaxLength(200);

        // One profile per Discord user.
        builder.HasIndex(e => e.DiscordUserId).IsUnique();

        builder.HasMany(e => e.Submissions)
            .WithOne(s => s.EditorProfile)
            .HasForeignKey(s => s.EditorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
