using Couture.Notifications.Domain;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Persistence;

public sealed class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationConfig> NotificationConfigs => Set<NotificationConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
    }
}
