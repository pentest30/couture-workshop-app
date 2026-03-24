using Couture.Dashboard.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Dashboard.Persistence;

public sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("holidays");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(h => h.Date);
    }
}
