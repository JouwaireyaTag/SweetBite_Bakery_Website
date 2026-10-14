using Microsoft.EntityFrameworkCore;

namespace SweetBite.Api;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = "";
    public string Category { get; set; } = "";
    public bool IsAvailable { get; set; } = true;
}

public class CustomerOrder
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Status { get; set; } = "Received";
    public decimal Total { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int CustomerOrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public record CreateOrderRequest(string CustomerName, string CustomerEmail, string? Notes, List<CreateOrderItem>? Items);
public record CreateOrderItem(int ProductId, int Quantity);

public class SweetBiteDbContext(DbContextOptions<SweetBiteDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CustomerOrder> Orders => Set<CustomerOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(10, 2);
        modelBuilder.Entity<CustomerOrder>().Property(o => o.Total).HasPrecision(10, 2);
        modelBuilder.Entity<OrderItem>().Property(i => i.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<CustomerOrder>().HasMany(o => o.Items)
            .WithOne().HasForeignKey(i => i.CustomerOrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
