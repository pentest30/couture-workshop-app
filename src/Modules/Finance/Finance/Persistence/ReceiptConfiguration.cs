using Couture.Finance.Contracts;
using Couture.Finance.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Finance.Persistence;

public sealed class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("receipts");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasConversion(id => id.Value, v => ReceiptId.From(v));
        builder.Property(r => r.PaymentId).HasConversion(id => id.Value, v => PaymentId.From(v));
        builder.Property(r => r.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(r => r.Code).IsUnique();
        builder.Property(r => r.PdfStoragePath).HasMaxLength(500).IsRequired();
    }
}
