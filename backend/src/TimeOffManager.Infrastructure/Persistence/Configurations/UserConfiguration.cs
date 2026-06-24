using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeOffManager.Domain.Entities;

namespace TimeOffManager.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique(); // deterministic login, no duplicates

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(120);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.AnnualVacationDays).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();

        builder.HasMany(u => u.Requests)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict); // deleting a user never destroys history

        builder.Metadata
            .FindNavigation(nameof(User.Requests))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
