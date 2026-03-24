using Couture.Finance.Contracts;
using Couture.Finance.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Finance.Persistence;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasConversion(id => id.Value, v => PaymentId.From(v));
        builder.Property(p => p.Amount).HasPrecision(12, 2);
        builder.Property(p => p.PaymentMethod).HasConversion(m => m.Value, v => PaymentMethod.FromValue(v)).IsRequired();
        builder.Property(p => p.Note).HasMaxLength(500);
        builder.HasIndex(p => p.OrderId);
        builder.HasOne(p => p.Receipt).WithOne().HasForeignKey<Receipt>(r => r.PaymentId);
    }
}
