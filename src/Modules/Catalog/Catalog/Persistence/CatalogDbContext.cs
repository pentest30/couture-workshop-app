using Couture.Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Persistence;

public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }
    public DbSet<Model> Models => Set<Model>();
    public DbSet<ModelPhoto> ModelPhotos => Set<ModelPhoto>();
    public DbSet<ModelFabric> ModelFabrics => Set<ModelFabric>();
    public DbSet<Fabric> Fabrics => Set<Fabric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.Entity<Model>(b =>
        {
            b.ToTable("models");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).HasConversion(id => id.Value, v => Contracts.ModelId.From(v));
            b.Property(m => m.Category).HasConversion(c => c.Value, v => ModelCategory.FromValue(v)).IsRequired();
            b.Property(m => m.Code).HasMaxLength(20).IsRequired();
            b.HasIndex(m => m.Code).IsUnique();
            b.Property(m => m.Name).HasMaxLength(200).IsRequired();
            b.Property(m => m.WorkType).HasMaxLength(50).IsRequired();
            b.Property(m => m.BasePrice).HasPrecision(12, 2);
            b.Property(m => m.Description).HasMaxLength(2000);
            b.HasMany(m => m.Photos).WithOne().HasForeignKey(p => p.ModelId);
            b.HasMany(m => m.ModelFabrics).WithOne().HasForeignKey(mf => mf.ModelId);
            b.Navigation(m => m.Photos).UsePropertyAccessMode(PropertyAccessMode.Field);
            b.Navigation(m => m.ModelFabrics).UsePropertyAccessMode(PropertyAccessMode.Field);
            b.Property(m => m.IsActive).HasDefaultValue(true);
            b.HasQueryFilter(m => m.IsActive);
        });

        modelBuilder.Entity<ModelPhoto>(b =>
        {
            b.ToTable("model_photos");
            b.HasKey(p => p.Id);
            b.Property(p => p.ModelId).HasConversion(id => id.Value, v => Contracts.ModelId.From(v));
            b.Property(p => p.FileName).HasMaxLength(500).IsRequired();
            b.Property(p => p.StoragePath).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<ModelFabric>(b =>
        {
            b.ToTable("model_fabrics");
            b.HasKey(mf => mf.Id);
            b.Property(mf => mf.ModelId).HasConversion(id => id.Value, v => Contracts.ModelId.From(v));
            b.Property(mf => mf.FabricId).HasConversion(id => id.Value, v => Contracts.FabricId.From(v));
            b.HasIndex(mf => new { mf.ModelId, mf.FabricId }).IsUnique();
        });

        modelBuilder.Entity<Fabric>(b =>
        {
            b.ToTable("fabrics");
            b.HasKey(f => f.Id);
            b.Property(f => f.Id).HasConversion(id => id.Value, v => Contracts.FabricId.From(v));
            b.Property(f => f.Name).HasMaxLength(200).IsRequired();
            b.Property(f => f.Type).HasMaxLength(100).IsRequired();
            b.Property(f => f.Color).HasMaxLength(100).IsRequired();
            b.Property(f => f.Supplier).HasMaxLength(200);
            b.Property(f => f.PricePerMeter).HasPrecision(12, 2);
            b.Property(f => f.StockMeters).HasPrecision(12, 2);
            b.Property(f => f.Description).HasMaxLength(2000);
            b.Property(f => f.SwatchPath).HasMaxLength(1000);
        });
    }
}
