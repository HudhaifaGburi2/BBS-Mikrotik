using BroadbandBilling.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(u => u.UserType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);
        
        builder.Property(u => u.LastLoginIpAddress)
            .HasMaxLength(50);
        
        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);
        
        // Indexes
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.UserType);
        builder.HasIndex(u => u.SubscriberId);
        
        // Relationships
        builder.HasOne(u => u.Subscriber)
            .WithOne(s => s.User)
            .HasForeignKey<User>(u => u.SubscriberId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(u => u.Admin)
            .WithOne(a => a.User)
            .HasForeignKey<Admin>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(u => u.LoginHistory)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
