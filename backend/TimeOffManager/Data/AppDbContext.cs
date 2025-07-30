using Microsoft.EntityFrameworkCore;
using TimeOffManager.Models;

namespace TimeOffManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TimeOffRequest> TimeOffRequests => Set<TimeOffRequest>();


    
}
