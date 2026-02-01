using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class PppoeAccountConfiguration : IEntityTypeConfiguration<PppoeAccount>
{
    public void Configure(EntityTypeBuilder<PppoeAccount> builder)
    {
        builder.ToTable("PppoeAccounts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.SubscriberId)
            .IsRequired();

        builder.Property(p => p.SubscriptionId)
            .IsRequired();

        builder.Property(p => p.MikroTikDeviceId)
            .IsRequired();

        builder.Property(p => p.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Password)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(p => p.StaticIpAddress, ip =>
        {
            ip.Property(i => i.Value)
                .HasColumnName("StaticIpAddress")
                .HasMaxLength(45);
        });

        builder.Property(p => p.ProfileName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.IsEnabled)
            .IsRequired();

        builder.Property(p => p.LastConnectedAt);

        builder.Property(p => p.LastDisconnectedAt);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        builder.HasOne(p => p.Subscriber)
            .WithMany()
            .HasForeignKey(p => p.SubscriberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Subscription)
            .WithMany()
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.MikroTikDevice)
            .WithMany()
            .HasForeignKey(p => p.MikroTikDeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Username)
            .IsUnique();

        builder.HasIndex(p => p.SubscriberId);

        builder.HasIndex(p => p.SubscriptionId);

        builder.HasIndex(p => p.IsEnabled);
    }
}
