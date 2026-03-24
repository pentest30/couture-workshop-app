using Couture.Orders.Contracts;
using Couture.Orders.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Orders.Persistence;

public sealed class StatusTransitionConfiguration : IEntityTypeConfiguration<StatusTransition>
{
    public void Configure(EntityTypeBuilder<StatusTransition> builder)
    {
        builder.ToTable("order_status_transitions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, v => StatusTransitionId.From(v));

        builder.Property(t => t.OrderId)
            .HasConversion(id => id.Value, v => OrderId.From(v));

        builder.Property(t => t.FromStatus)
            .HasConversion(s => s != null ? s.Value : (int?)null, v => v.HasValue ? OrderStatus.FromValue(v.Value) : null);

        builder.Property(t => t.ToStatus)
            .HasConversion(s => s.Value, v => OrderStatus.FromValue(v))
            .IsRequired();

        builder.Property(t => t.Reason).HasMaxLength(500);
        builder.Property(t => t.TransitionedBy).HasMaxLength(100).IsRequired();

        builder.HasIndex(t => new { t.OrderId, t.TransitionedAt });
    }
}
