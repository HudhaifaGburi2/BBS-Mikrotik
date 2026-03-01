using BroadbandBilling.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("LoginHistory");
        
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.UserType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(l => l.LoginStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(l => l.FailureReason)
            .HasColumnType("NVARCHAR(200)");
        
        builder.Property(l => l.IpAddress)
            .HasMaxLength(50);
        
        builder.Property(l => l.DeviceName)
            .HasColumnType("NVARCHAR(200)");
        
        builder.Property(l => l.Browser)
            .HasMaxLength(500);
        
        builder.Property(l => l.OperatingSystem)
            .HasMaxLength(100);
        
        builder.Property(l => l.Location)
            .HasColumnType("NVARCHAR(200)");
        
        // Indexes
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.LoginDate);
        builder.HasIndex(l => l.LoginStatus);
        
        // Relationships configured in UserConfiguration
    }
}
