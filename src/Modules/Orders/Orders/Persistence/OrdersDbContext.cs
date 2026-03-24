using Couture.Orders.Domain;
using Microsoft.EntityFrameworkCore;

namespace Couture.Orders.Persistence;

public sealed class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<StatusTransition> StatusTransitions => Set<StatusTransition>();
    public DbSet<OrderPhoto> OrderPhotos => Set<OrderPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
    }
}
