using BroadbandBilling.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Gateway)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pt => pt.SessionId)
            .HasMaxLength(500);

        builder.Property(pt => pt.GatewayTransactionId)
            .HasMaxLength(500);

        builder.Property(pt => pt.GatewayOrderId)
            .HasMaxLength(500);

        builder.Property(pt => pt.Amount)
            .HasPrecision(18, 2);

        builder.Property(pt => pt.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(pt => pt.Status)
            .IsRequired();

        builder.Property(pt => pt.CardBrand)
            .HasMaxLength(50);

        builder.Property(pt => pt.CardLast4)
            .HasMaxLength(4);

        builder.Property(pt => pt.RawRequest)
            .HasColumnType("nvarchar(max)");

        builder.Property(pt => pt.RawResponse)
            .HasColumnType("nvarchar(max)");

        builder.Property(pt => pt.FailureReason)
            .HasMaxLength(1000);

        builder.HasOne(pt => pt.Subscription)
            .WithMany(s => s.PaymentTransactions)
            .HasForeignKey(pt => pt.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pt => pt.Subscriber)
            .WithMany()
            .HasForeignKey(pt => pt.SubscriberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pt => pt.SubscriptionId);
        builder.HasIndex(pt => pt.SubscriberId);
        builder.HasIndex(pt => pt.SessionId);
        builder.HasIndex(pt => pt.GatewayTransactionId);
        builder.HasIndex(pt => pt.Status);
    }
}
