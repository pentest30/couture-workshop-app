using Couture.Clients.Contracts;
using Couture.Clients.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Clients.Persistence;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(id => id.Value, v => ClientId.From(v));
        builder.Property(c => c.Code).HasMaxLength(10).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.PrimaryPhone).HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.PrimaryPhone);
        builder.Property(c => c.SecondaryPhone).HasMaxLength(20);
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.Notes).HasMaxLength(2000);
    }
}
