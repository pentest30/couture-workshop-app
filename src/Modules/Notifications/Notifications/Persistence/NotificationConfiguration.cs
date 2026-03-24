using Couture.Notifications.Contracts;
using Couture.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Couture.Notifications.Persistence;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasConversion(id => id.Value, v => NotificationId.From(v));
        builder.Property(n => n.Type).HasConversion(t => t.Value, v => NotificationType.FromValue(v)).IsRequired();
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();
        builder.HasIndex(n => new { n.RecipientId, n.IsRead });
        builder.HasIndex(n => n.ExpiresAt);
    }
}
