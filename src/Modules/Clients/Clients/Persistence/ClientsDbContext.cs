using Couture.Clients.Domain;
using Microsoft.EntityFrameworkCore;

namespace Couture.Clients.Persistence;

public sealed class ClientsDbContext : DbContext
{
    public ClientsDbContext(DbContextOptions<ClientsDbContext> options) : base(options) { }
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<MeasurementField> MeasurementFields => Set<MeasurementField>();
    public DbSet<ClientMeasurement> ClientMeasurements => Set<ClientMeasurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("clients");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClientsDbContext).Assembly);
    }
}
