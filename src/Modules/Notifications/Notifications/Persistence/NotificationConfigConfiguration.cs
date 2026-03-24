using Couture.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Notifications.Persistence;

public sealed class NotificationConfigConfiguration : IEntityTypeConfiguration<NotificationConfig>
{
    public void Configure(EntityTypeBuilder<NotificationConfig> builder)
    {
        builder.ToTable("notification_configs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Type).HasConversion(t => t.Value, v => NotificationType.FromValue(v)).IsRequired();
        builder.HasIndex(c => c.Type).IsUnique();
    }
}
