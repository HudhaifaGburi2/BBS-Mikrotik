using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SubscriberId)
            .IsRequired();

        builder.Property(s => s.PlanId)
            .IsRequired();

        builder.OwnsOne(s => s.BillingPeriod, period =>
        {
            period.Property(d => d.StartDate)
                .HasColumnName("StartDate")
                .IsRequired();

            period.Property(d => d.EndDate)
                .HasColumnName("EndDate")
                .IsRequired();
        });

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.ActivatedAt);

        builder.Property(s => s.SuspendedAt);

        builder.Property(s => s.CancelledAt);

        builder.Property(s => s.CancellationReason)
            .HasColumnType("NVARCHAR(500)");

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.HasOne(s => s.Subscriber)
            .WithMany(sub => sub.Subscriptions)
            .HasForeignKey(s => s.SubscriberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Plan)
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.SubscriberId);

        builder.HasIndex(s => s.PlanId);

        builder.HasIndex(s => s.Status);

        builder.HasIndex(s => new { s.SubscriberId, s.Status });
    }
}
