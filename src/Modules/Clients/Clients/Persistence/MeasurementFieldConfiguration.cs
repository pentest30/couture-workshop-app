using Couture.Clients.Contracts;
using Couture.Clients.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Clients.Persistence;

public sealed class MeasurementFieldConfiguration : IEntityTypeConfiguration<MeasurementField>
{
    public void Configure(EntityTypeBuilder<MeasurementField> builder)
    {
        builder.ToTable("measurement_fields");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasConversion(id => id.Value, v => MeasurementFieldId.From(v));
        builder.Property(f => f.Name).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Unit).HasMaxLength(10).IsRequired();
    }
}
