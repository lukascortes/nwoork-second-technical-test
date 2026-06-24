using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TimeOffManager.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c> so migrations can be generated
/// without the API project or a live database. The connection string here is a
/// placeholder; it is never used at runtime.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=timeoff;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }
}
