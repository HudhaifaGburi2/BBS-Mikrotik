using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.InvoiceId)
            .IsRequired();

        builder.Property(p => p.SubscriberId)
            .IsRequired();

        builder.Property(p => p.PaymentReference)
            .IsRequired()
            .HasColumnType("NVARCHAR(100)");

        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.TransactionId)
            .HasColumnType("NVARCHAR(200)");

        builder.Property(p => p.Notes)
            .HasColumnType("NVARCHAR(1000)");

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Subscriber)
            .WithMany()
            .HasForeignKey(p => p.SubscriberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.PaymentReference)
            .IsUnique();

        builder.HasIndex(p => p.InvoiceId);

        builder.HasIndex(p => p.SubscriberId);

        builder.HasIndex(p => p.Status);

        builder.HasIndex(p => p.PaymentDate);
    }
}
