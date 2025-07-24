using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;

namespace InventoryAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        //sets
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Inventory> Inventory { get; set; } = null!;
        public DbSet<Warehouse> Warehouses { get; set; } = null!;
        public DbSet<StockLog> StockLogs { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity properties, relationships, etc.
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Inventory>().ToTable("Inventory");
            modelBuilder.Entity<Warehouse>().ToTable("Warehouses");
            modelBuilder.Entity<StockLog>().ToTable("StockLogs");
            modelBuilder.Entity<User>().ToTable("Users");

            // Additional configurations can be added here

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });

            // StockLog relationships
            // modelBuilder.Entity<StockLog>()
            //     .HasOne(sl => sl.Product)
            //     .WithMany()
            //     .HasForeignKey(sl => sl.ProductId)
            //     .OnDelete(DeleteBehavior.Restrict);

            // modelBuilder.Entity<StockLog>()
            //     .HasOne(sl => sl.Warehouse)
            //     .WithMany()
            //     .HasForeignKey(sl => sl.WarehouseId)
            //     .OnDelete(DeleteBehavior.Restrict);

            // modelBuilder.Entity<StockLog>()
            //     .HasOne(sl => sl.User)
            //     .WithMany()
            //     .HasForeignKey(sl => sl.UserId)
            //     .OnDelete(DeleteBehavior.SetNull);
        }
    }
}