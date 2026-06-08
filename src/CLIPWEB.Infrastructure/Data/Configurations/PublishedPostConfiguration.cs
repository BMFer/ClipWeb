using CLIPWEB.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CLIPWEB.Infrastructure.Data.Configurations;

public class PublishedPostConfiguration : IEntityTypeConfiguration<PublishedPost>
{
    public void Configure(EntityTypeBuilder<PublishedPost> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Platform).IsRequired().HasMaxLength(50);
        builder.Property(p => p.PostUrl).IsRequired().HasMaxLength(500);
    }
}
