using Couture.Clients.Contracts;
using Couture.Clients.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Clients.Persistence;

public sealed class ClientMeasurementConfiguration : IEntityTypeConfiguration<ClientMeasurement>
{
    public void Configure(EntityTypeBuilder<ClientMeasurement> builder)
    {
        builder.ToTable("client_measurements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasConversion(id => id.Value, v => ClientMeasurementId.From(v));
        builder.Property(m => m.ClientId).HasConversion(id => id.Value, v => ClientId.From(v));
        builder.Property(m => m.MeasurementFieldId).HasConversion(id => id.Value, v => MeasurementFieldId.From(v));
        builder.Property(m => m.Value).HasPrecision(6, 1);
    }
}
