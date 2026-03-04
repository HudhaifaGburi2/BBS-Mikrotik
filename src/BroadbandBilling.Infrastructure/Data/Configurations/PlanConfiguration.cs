using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasColumnType("NVARCHAR(100)");

        builder.Property(p => p.Description)
            .HasColumnType("NVARCHAR(500)");

        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2)
                .IsRequired();

            price.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(p => p.SpeedMbps)
            .IsRequired();

        builder.Property(p => p.DataLimitGB)
            .IsRequired();

        builder.Property(p => p.BillingCycleDays)
            .IsRequired();

        builder.Property(p => p.BillingCycleHours);

        builder.Property(p => p.MikroTikProfileName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasIndex(p => p.IsActive);
    }
}
