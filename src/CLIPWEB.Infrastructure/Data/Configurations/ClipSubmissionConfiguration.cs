using CLIPWEB.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CLIPWEB.Infrastructure.Data.Configurations;

public class ClipSubmissionConfiguration : IEntityTypeConfiguration<ClipSubmission>
{
    public void Configure(EntityTypeBuilder<ClipSubmission> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ClipUrl).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Notes).HasMaxLength(2000);
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(s => s.Status);

        builder.HasMany(s => s.PublishedPosts)
            .WithOne(p => p.ClipSubmission)
            .HasForeignKey(p => p.ClipSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
