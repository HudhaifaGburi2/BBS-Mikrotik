using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Infrastructure.Data.Configurations;

public class MikroTikDeviceConfiguration : IEntityTypeConfiguration<MikroTikDevice>
{
    public void Configure(EntityTypeBuilder<MikroTikDevice> builder)
    {
        builder.ToTable("MikroTikDevices");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasColumnType("NVARCHAR(100)");

        builder.OwnsOne(m => m.IpAddress, ip =>
        {
            ip.Property(i => i.Value)
                .HasColumnName("IpAddress")
                .HasMaxLength(45)
                .IsRequired();
        });

        builder.Property(m => m.Port)
            .IsRequired();

        builder.Property(m => m.Username)
            .IsRequired()
            .HasColumnType("NVARCHAR(100)");

        builder.Property(m => m.Password)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.IsActive)
            .IsRequired();

        builder.Property(m => m.Location)
            .HasColumnType("NVARCHAR(200)");

        builder.Property(m => m.LastConnectedAt);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt);

        builder.HasIndex(m => m.Name)
            .IsUnique();

        builder.HasIndex(m => m.IsActive);
    }
}
