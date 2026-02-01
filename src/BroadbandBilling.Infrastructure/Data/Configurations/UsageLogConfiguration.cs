using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class UsageLogConfiguration : IEntityTypeConfiguration<UsageLog>
{
    public void Configure(EntityTypeBuilder<UsageLog> builder)
    {
        builder.ToTable("UsageLogs");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.SubscriberId)
            .IsRequired();

        builder.Property(u => u.SubscriptionId)
            .IsRequired();

        builder.Property(u => u.PppoeAccountId)
            .IsRequired();

        builder.Property(u => u.SessionStart)
            .IsRequired();

        builder.Property(u => u.SessionEnd);

        builder.Property(u => u.UploadBytes)
            .IsRequired();

        builder.Property(u => u.DownloadBytes)
            .IsRequired();

        builder.Property(u => u.TotalBytes)
            .IsRequired();

        builder.OwnsOne(u => u.IpAddress, ip =>
        {
            ip.Property(i => i.Value)
                .HasColumnName("IpAddress")
                .HasMaxLength(45);
        });

        builder.Property(u => u.CallingStationId)
            .HasMaxLength(100);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.HasOne(u => u.Subscriber)
            .WithMany()
            .HasForeignKey(u => u.SubscriberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Subscription)
            .WithMany()
            .HasForeignKey(u => u.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.PppoeAccount)
            .WithMany()
            .HasForeignKey(u => u.PppoeAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.SubscriberId);

        builder.HasIndex(u => u.SubscriptionId);

        builder.HasIndex(u => u.PppoeAccountId);

        builder.HasIndex(u => u.SessionStart);

        builder.HasIndex(u => new { u.SubscriberId, u.SessionStart });
    }
}
