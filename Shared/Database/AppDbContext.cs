using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Models;

namespace Recyclage.Shared.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<AppSettingsRow> AppSettings => Set<AppSettingsRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSettingsRow>(e =>
        {
            e.Property(s => s.CompanyCapital).HasPrecision(18, 2);
            e.Property(s => s.BackupIntervalUnit).IsRequired().HasMaxLength(16);
            e.Property(s => s.BackupDirectory).IsRequired();
        });

        modelBuilder.Entity<Client>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Phone).IsRequired().HasMaxLength(32);
            e.Property(c => c.Ice).IsRequired().HasMaxLength(32);
            e.HasIndex(c => c.Name);
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.Phone).IsRequired().HasMaxLength(32);
            e.Property(s => s.Ice).IsRequired().HasMaxLength(32);
            e.HasIndex(s => s.Name);
        });

        modelBuilder.Entity<PurchaseInvoice>(e =>
        {
            e.Property(p => p.Date).IsRequired().HasMaxLength(10);
            e.Property(p => p.Quantity).HasPrecision(18, 3);
            e.Property(p => p.UnitPrice).HasPrecision(18, 2);
            e.Property(p => p.TransportCost).HasPrecision(18, 2);
            e.Property(p => p.Total).HasPrecision(18, 2);
            e.Property(p => p.Paid).HasPrecision(18, 2);
            e.Property(p => p.Remaining).HasPrecision(18, 2);
            e.HasOne(p => p.Product).WithMany().HasForeignKey(p => p.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Supplier).WithMany().HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => p.SupplierId);
            e.HasIndex(p => p.Date);
        });

        modelBuilder.Entity<Sale>(e =>
        {
            e.Property(s => s.Date).IsRequired().HasMaxLength(10);
            e.Property(s => s.Quantity).HasPrecision(18, 3);
            e.Property(s => s.UnitPrice).HasPrecision(18, 2);
            e.Property(s => s.TransportCost).HasPrecision(18, 2);
            e.Property(s => s.Total).HasPrecision(18, 2);
            e.Property(s => s.Paid).HasPrecision(18, 2);
            e.Property(s => s.Remaining).HasPrecision(18, 2);
            e.HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Client).WithMany().HasForeignKey(s => s.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => s.ClientId);
            e.HasIndex(s => s.Date);
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.Property(x => x.Date).IsRequired().HasMaxLength(10);
            e.Property(x => x.ExpenseType).IsRequired().HasMaxLength(200);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Description).IsRequired().HasMaxLength(500);
            e.HasIndex(x => x.Date);
            e.HasIndex(x => x.ExpenseType);
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
