using Microsoft.EntityFrameworkCore;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Infrastructure.Persistence;

/// <summary>Seeds demo data (one admin, two employees, a few requests) on an empty
/// database, so the live demo is immediately explorable. Credentials are documented
/// in the README and are intended for the public demo only.</summary>
public static class AppDbContextSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
            return;

        var admin = User.Create("admin@timeoff.dev", hasher.Hash("Admin123!"), "Alex Admin", UserRole.Admin, 30);
        var emma = User.Create("emma@timeoff.dev", hasher.Hash("Employee123!"), "Emma Stone", UserRole.Employee, 20);
        var liam = User.Create("liam@timeoff.dev", hasher.Hash("Employee123!"), "Liam Carter", UserRole.Employee, 20);

        await db.Users.AddRangeAsync(new[] { admin, emma, liam }, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var vacation = TimeOffRequest.Create(emma.Id, today.AddDays(10), today.AddDays(14), LeaveType.Vacation, "Family trip");
        var sick = TimeOffRequest.Create(emma.Id, today.AddDays(40), today.AddDays(41), LeaveType.Sick, "Medical appointment");
        var approved = TimeOffRequest.Create(liam.Id, today.AddDays(20), today.AddDays(22), LeaveType.Vacation, "Wedding");
        approved.Approve(admin.Id, DateTime.UtcNow);

        await db.TimeOffRequests.AddRangeAsync(new[] { vacation, sick, approved }, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }
}
