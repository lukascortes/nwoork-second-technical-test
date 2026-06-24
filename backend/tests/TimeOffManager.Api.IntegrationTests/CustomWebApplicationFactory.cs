using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Infrastructure.Persistence;

namespace TimeOffManager.Api.IntegrationTests;

/// <summary>
/// Boots the real API in-process (real controllers, auth, pipeline) but swaps
/// PostgreSQL for an in-memory SQLite database, so the full HTTP stack can be
/// tested without Docker or a live database.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // loads appsettings.Testing.json (test Jwt key, etc.)

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            _connection.Open(); // keep the in-memory DB alive for the factory's lifetime
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    /// <summary>Recreates the schema and re-seeds the demo data.</summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await AppDbContextSeeder.SeedAsync(db, hasher);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
