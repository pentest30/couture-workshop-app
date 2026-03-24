using Couture.Orders.Contracts;
using Couture.Orders.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Orders.Persistence;

public sealed class OrderPhotoConfiguration : IEntityTypeConfiguration<OrderPhoto>
{
    public void Configure(EntityTypeBuilder<OrderPhoto> builder)
    {
        builder.ToTable("order_photos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, v => OrderPhotoId.From(v));

        builder.Property(p => p.OrderId)
            .HasConversion(id => id.Value, v => OrderId.From(v));

        builder.Property(p => p.FileName).HasMaxLength(255).IsRequired();
        builder.Property(p => p.StoragePath).HasMaxLength(500).IsRequired();
    }
}
