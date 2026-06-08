using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Models;

namespace RestaurantBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Discount> Discounts => Set<Discount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // decimal ko sqlite me theek se store karne ke liye
        modelBuilder.Entity<MenuItem>().Property(p => p.Price).HasConversion<double>();
        modelBuilder.Entity<Order>().Property(p => p.TotalAmount).HasConversion<double>();
        modelBuilder.Entity<Order>().Property(p => p.Subtotal).HasConversion<double>();
        modelBuilder.Entity<Order>().Property(p => p.TaxAmount).HasConversion<double>();
        modelBuilder.Entity<Order>().Property(p => p.DiscountAmount).HasConversion<double>();
        modelBuilder.Entity<OrderItem>().Property(p => p.Price).HasConversion<double>();
        modelBuilder.Entity<Discount>().Property(p => p.Percentage).HasConversion<double>();

        // Order delete ho to uske items bhi delete
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order!)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
