using Couture.Orders.Contracts;
using Couture.Orders.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Orders.Persistence;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, v => OrderId.From(v));

        builder.Property(o => o.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(o => o.Code).IsUnique();

        builder.Property(o => o.Status)
            .HasConversion(s => s.Value, v => OrderStatus.FromValue(v))
            .IsRequired();

        builder.Property(o => o.WorkType)
            .HasConversion(w => w.Value, v => WorkType.FromValue(v))
            .IsRequired();

        builder.Property(o => o.Description).HasMaxLength(2000);
        builder.Property(o => o.Fabric).HasMaxLength(500);
        builder.Property(o => o.TechnicalNotes).HasMaxLength(2000);
        builder.Property(o => o.EmbroideryStyle).HasMaxLength(500);
        builder.Property(o => o.ThreadColors).HasMaxLength(500);
        builder.Property(o => o.Density).HasMaxLength(200);
        builder.Property(o => o.EmbroideryZone).HasMaxLength(500);
        builder.Property(o => o.BeadType).HasMaxLength(500);
        builder.Property(o => o.Arrangement).HasMaxLength(500);
        builder.Property(o => o.AffectedZones).HasMaxLength(500);
        builder.Property(o => o.TotalPrice).HasPrecision(12, 2);
        builder.Property(o => o.DeliveryWithUnpaidReason).HasMaxLength(500);

        builder.Property(o => o.RowVersion).IsRowVersion();

        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.ClientId);
        builder.HasIndex(o => o.ExpectedDeliveryDate);
        builder.HasIndex(o => o.AssignedTailorId);
        builder.HasIndex(o => o.AssignedEmbroidererId);
        builder.HasIndex(o => o.AssignedBeaderId);
        builder.HasIndex(o => o.ReceptionDate);

        builder.HasMany(o => o.Transitions).WithOne().HasForeignKey(t => t.OrderId);
        builder.HasMany(o => o.Photos).WithOne().HasForeignKey(p => p.OrderId);

        builder.Navigation(o => o.Transitions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.Photos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
