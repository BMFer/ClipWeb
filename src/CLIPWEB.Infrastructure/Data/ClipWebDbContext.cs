using CLIPWEB.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLIPWEB.Infrastructure.Data;

/// <summary>
/// EF Core database context for CLIPWEB.
/// </summary>
public class ClipWebDbContext : DbContext
{
    public ClipWebDbContext(DbContextOptions<ClipWebDbContext> options)
        : base(options)
    {
    }

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<EditorProfile> EditorProfiles => Set<EditorProfile>();
    public DbSet<ClipSubmission> ClipSubmissions => Set<ClipSubmission>();
    public DbSet<PublishedPost> PublishedPosts => Set<PublishedPost>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClipWebDbContext).Assembly);
    }
}
