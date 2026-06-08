using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CLIPWEB.Infrastructure.Data;

/// <summary>
/// Design-time factory so EF Core CLI tooling (migrations) can create a
/// <see cref="ClipWebDbContext"/> without running the host.
/// Used only by <c>dotnet ef</c>; the runtime uses DI configuration instead.
/// </summary>
public class ClipWebDbContextFactory : IDesignTimeDbContextFactory<ClipWebDbContext>
{
    public ClipWebDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CLIPWEB_CONNECTION")
            ?? "Data Source=clipweb.db";

        var options = new DbContextOptionsBuilder<ClipWebDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new ClipWebDbContext(options);
    }
}
