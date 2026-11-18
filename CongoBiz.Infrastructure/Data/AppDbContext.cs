using CongoBiz.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CongoBiz.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
public DbSet<Customer> Customers => Set<Customer>();
public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
public DbSet<StockMovement> StockMovements => Set<StockMovement>();
public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // required for Identity tables

        builder.Entity<Restaurant>(e =>
        {
            e.HasIndex(r => r.Slug).IsUnique();
            e.Property(r => r.Name).HasMaxLength(200).IsRequired();
        });

        builder.Entity<MenuCategory>(e =>
        {
            e.HasOne(mc => mc.Restaurant)
             .WithMany(r => r.MenuCategories)
             .HasForeignKey(mc => mc.RestaurantId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MenuItem>(e =>
        {
            e.Property(mi => mi.Price).HasColumnType("decimal(10,2)");
            e.HasOne(mi => mi.Category)
             .WithMany(mc => mc.MenuItems)
             .HasForeignKey(mi => mi.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<RestaurantTable>(e =>
{
    e.HasOne(t => t.Restaurant)
     .WithMany()
     .HasForeignKey(t => t.RestaurantId)
     .OnDelete(DeleteBehavior.Cascade);
});

        builder.Entity<Order>(e =>
        {
            e.Property(o => o.Subtotal).HasColumnType("decimal(10,2)");
            e.Property(o => o.Tax).HasColumnType("decimal(10,2)");
            e.Property(o => o.Discount).HasColumnType("decimal(10,2)");
            e.Property(o => o.Total).HasColumnType("decimal(10,2)");

            e.HasOne(o => o.Restaurant)
             .WithMany()
             .HasForeignKey(o => o.RestaurantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.Table)
             .WithMany()
             .HasForeignKey(o => o.TableId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrderItem>(e =>
        {
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(10,2)");
            e.Property(oi => oi.Subtotal).HasColumnType("decimal(10,2)");

            e.HasOne(oi => oi.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(oi => oi.MenuItem)
             .WithMany()
             .HasForeignKey(oi => oi.MenuItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Customer>(e =>
{
    e.HasOne(c => c.Restaurant)
     .WithMany()
     .HasForeignKey(c => c.RestaurantId)
     .OnDelete(DeleteBehavior.Restrict);
});

builder.Entity<InventoryItem>(e =>
{
    e.Property(i => i.Quantity).HasColumnType("decimal(10,2)");
    e.Property(i => i.MinimumQuantity).HasColumnType("decimal(10,2)");
    e.Property(i => i.CostPerUnit).HasColumnType("decimal(10,2)");

    e.HasOne(i => i.Restaurant)
     .WithMany()
     .HasForeignKey(i => i.RestaurantId)
     .OnDelete(DeleteBehavior.Restrict);
});

builder.Entity<StockMovement>(e =>
{
    e.Property(s => s.Quantity).HasColumnType("decimal(10,2)");

    e.HasOne(s => s.InventoryItem)
     .WithMany()
     .HasForeignKey(s => s.InventoryItemId)
     .OnDelete(DeleteBehavior.Cascade);
});

builder.Entity<Expense>(e =>
{
    e.Property(x => x.Amount).HasColumnType("decimal(10,2)");

    e.HasOne<Restaurant>()
     .WithMany()
     .HasForeignKey(x => x.RestaurantId)
     .OnDelete(DeleteBehavior.Restrict);
});
    }
}