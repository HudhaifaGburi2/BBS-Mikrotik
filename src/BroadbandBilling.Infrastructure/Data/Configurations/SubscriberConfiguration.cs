using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class SubscriberConfiguration : IEntityTypeConfiguration<Subscriber>
{
    public void Configure(EntityTypeBuilder<Subscriber> builder)
    {
        builder.ToTable("Subscribers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.FullName)
            .IsRequired()
            .HasColumnType("NVARCHAR(200)");

        builder.Property(s => s.Email)
            .IsRequired()
            .HasColumnType("NVARCHAR(100)");

        builder.Property(s => s.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Address)
            .IsRequired()
            .HasColumnType("NVARCHAR(500)");

        builder.Property(s => s.NationalId)
            .HasColumnType("NVARCHAR(50)");
        
        builder.Property(s => s.City)
            .HasColumnType("NVARCHAR(100)");
        
        builder.Property(s => s.PostalCode)
            .HasColumnType("NVARCHAR(20)");
        
        // Device Information
        builder.Property(s => s.LastLoginDevice)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(s => s.LastLoginBrowser)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(s => s.LastLoginOS)
            .HasColumnType("NVARCHAR(MAX)");
        
        // Network Information
        builder.Property(s => s.MacAddress)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(s => s.IpAddress)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(s => s.MikroTikUsername)
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);
        
        builder.Property(s => s.UserId)
            .IsRequired(false);

        builder.HasIndex(s => s.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
        builder.HasIndex(s => s.Email);
        builder.HasIndex(s => s.PhoneNumber);
        builder.HasIndex(s => s.NationalId);
        builder.HasIndex(s => s.IsActive);
        
        // Relationship configured in UserConfiguration
    }
}
