using Couture.Dashboard.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Dashboard.Persistence;

public sealed class WorkshopSettingsConfiguration : IEntityTypeConfiguration<WorkshopSettings>
{
    public void Configure(EntityTypeBuilder<WorkshopSettings> builder)
    {
        builder.ToTable("workshop_settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.WorkshopName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.LogoPath).HasMaxLength(500);
    }
}
