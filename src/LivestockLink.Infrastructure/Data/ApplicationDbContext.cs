using LivestockLink.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LivestockLink.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<RestaurantProfile> RestaurantProfiles => Set<RestaurantProfile>();
    public DbSet<SupplierProfile> SupplierProfiles => Set<SupplierProfile>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<CommissionRecord> CommissionRecords => Set<CommissionRecord>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<PlatformSetting> PlatformSettings => Set<PlatformSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            entity.Property(u => u.PhoneNumber).HasMaxLength(30).IsRequired();
        });

        // RestaurantProfile
        modelBuilder.Entity<RestaurantProfile>(entity =>
        {
            entity.HasOne(r => r.User)
                  .WithOne(u => u.RestaurantProfile)
                  .HasForeignKey<RestaurantProfile>(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(r => r.RestaurantName).HasMaxLength(150).IsRequired();
            entity.Property(r => r.City).HasMaxLength(80);
            entity.Property(r => r.State).HasMaxLength(80);
        });

        // SupplierProfile
        modelBuilder.Entity<SupplierProfile>(entity =>
        {
            entity.HasOne(s => s.User)
                  .WithOne(u => u.SupplierProfile)
                  .HasForeignKey<SupplierProfile>(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(s => s.BusinessName).HasMaxLength(150).IsRequired();
            entity.Property(s => s.State).HasMaxLength(80);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.SupplierProfile)
                  .WithMany(s => s.Products)
                  .HasForeignKey(p => p.SupplierProfileId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(p => p.PricePerUnit).HasPrecision(18, 2);
            entity.Property(p => p.Title).HasMaxLength(150).IsRequired();
            entity.Property(p => p.Unit).HasMaxLength(30).IsRequired();
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(o => o.RestaurantProfile)
                  .WithMany(r => r.Orders)
                  .HasForeignKey(o => o.RestaurantProfileId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Product)
                  .WithMany(p => p.Orders)
                  .HasForeignKey(o => o.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(o => o.UnitPriceAtOrder).HasPrecision(18, 2);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.OrderNumber).HasMaxLength(40).IsRequired();
            entity.HasIndex(o => o.OrderNumber).IsUnique();
        });

        // CommissionRecord (1-to-1 with Order, Unique OrderId prevents duplicate commission records)
        modelBuilder.Entity<CommissionRecord>(entity =>
        {
            entity.HasOne(c => c.Order)
                  .WithOne(o => o.CommissionRecord)
                  .HasForeignKey<CommissionRecord>(c => c.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => c.OrderId).IsUnique();
            entity.Property(c => c.TotalOrderValue).HasPrecision(18, 2);
            entity.Property(c => c.CommissionPercentage).HasPrecision(5, 2);
            entity.Property(c => c.CommissionAmount).HasPrecision(18, 2);
            entity.Property(c => c.SupplierNetPayout).HasPrecision(18, 2);
        });

        // Dispute (1-to-1 with Order)
        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasOne(d => d.Order)
                  .WithOne(o => o.Dispute)
                  .HasForeignKey<Dispute>(d => d.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(d => d.OrderId).IsUnique();
            entity.Property(d => d.Reason).HasMaxLength(150).IsRequired();
            entity.Property(d => d.Description).HasMaxLength(2000).IsRequired();
        });

        // PlatformSetting
        modelBuilder.Entity<PlatformSetting>(entity =>
        {
            entity.HasKey(s => s.Key);
            entity.Property(s => s.Key).HasMaxLength(100);
            entity.Property(s => s.Value).HasMaxLength(500);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(a => a.Action).HasMaxLength(100);
            entity.Property(a => a.EntityName).HasMaxLength(100);
        });
    }
}
