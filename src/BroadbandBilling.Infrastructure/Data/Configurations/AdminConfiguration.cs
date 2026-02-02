using BroadbandBilling.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("Admins");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.FullName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(a => a.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(a => a.Permissions)
            .HasColumnType("NVARCHAR(MAX)");
        
        // Indexes
        builder.HasIndex(a => a.UserId).IsUnique();
        builder.HasIndex(a => a.Role);
        
        // Relationships configured in UserConfiguration
    }
}
