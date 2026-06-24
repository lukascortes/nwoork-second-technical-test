using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeOffManager.Domain.Entities;

namespace TimeOffManager.Infrastructure.Persistence.Configurations;

public sealed class TimeOffRequestConfiguration : IEntityTypeConfiguration<TimeOffRequest>
{
    public void Configure(EntityTypeBuilder<TimeOffRequest> builder)
    {
        builder.ToTable("time_off_requests");
        builder.HasKey(r => r.Id);

        // DateOnly maps to PostgreSQL `date` (no time-zone component).
        builder.Property(r => r.StartDate).IsRequired();
        builder.Property(r => r.EndDate).IsRequired();

        builder.Property(r => r.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Reason).HasMaxLength(500);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.ReviewedAt);
        builder.Property(r => r.ReviewedByUserId);

        builder.HasIndex(r => new { r.UserId, r.Status });
        builder.HasIndex(r => new { r.StartDate, r.EndDate });
    }
}
