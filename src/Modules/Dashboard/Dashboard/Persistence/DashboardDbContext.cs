using Couture.Dashboard.Domain;
using Microsoft.EntityFrameworkCore;

namespace Couture.Dashboard.Persistence;

public sealed class DashboardDbContext : DbContext
{
    public DashboardDbContext(DbContextOptions<DashboardDbContext> options) : base(options) { }
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<WorkshopSettings> WorkshopSettings => Set<WorkshopSettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dashboard");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DashboardDbContext).Assembly);
    }
}
