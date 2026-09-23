using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Models;

namespace Recyclage.Shared.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<AppSettingsRow> AppSettings => Set<AppSettingsRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSettingsRow>(e =>
        {
            e.Property(s => s.CompanyCapital).HasPrecision(18, 2);
            e.Property(s => s.BackupIntervalUnit).IsRequired().HasMaxLength(16);
            e.Property(s => s.BackupDirectory).IsRequired();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.ProductType)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            e.ToTable(t => t.HasCheckConstraint(
                "CK_Products_ProductType",
                "ProductType IN ('ForBuying', 'ForSale')"));
            e.Property(p => p.DefaultUnit).IsRequired().HasMaxLength(32);
            e.Property(p => p.DefaultUnitPrice).HasPrecision(18, 2);
            e.HasIndex(p => new { p.Name, p.ProductType });
        });
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var utc = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utc;
        }
    }
}
