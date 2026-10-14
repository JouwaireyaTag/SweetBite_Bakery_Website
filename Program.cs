using Microsoft.EntityFrameworkCore;
using SweetBite.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SweetBiteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SweetBite")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:SweetBite.")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

app.UseCors("Frontend");
app.UseDefaultFiles();
app.UseStaticFiles();

// For a learning/demo project, create the database and seed sample products automatically.
// For production, use EF Core migrations and a proper deployment process.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SweetBiteDbContext>();
    db.Database.EnsureCreated();

    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Name = "Butter Croissant", Description = "Flaky, buttery and freshly baked.", Price = 2.50m, ImageUrl = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=80", Category = "Pastry", IsAvailable = true },
            new Product { Name = "Chocolate Muffin", Description = "Rich chocolate muffin with chocolate chips.", Price = 3.00m, ImageUrl = "https://images.unsplash.com/photo-1607958996333-41aef7caefaa?auto=format&fit=crop&w=800&q=80", Category = "Muffin", IsAvailable = true },
            new Product { Name = "Cinnamon Roll", Description = "Soft cinnamon roll with sweet icing.", Price = 3.50m, ImageUrl = "https://images.unsplash.com/photo-1509365465985-25d11c17e812?auto=format&fit=crop&w=800&q=80", Category = "Pastry", IsAvailable = true },
            new Product { Name = "Fresh Baguette", Description = "A crisp crust with a soft centre.", Price = 2.00m, ImageUrl = "https://images.unsplash.com/photo-1585478259715-876acc5be8eb?auto=format&fit=crop&w=800&q=80", Category = "Bread", IsAvailable = true },
            new Product { Name = "Chocolate Chip Cookies", Description = "Golden cookies packed with chocolate chips.", Price = 1.50m, ImageUrl = "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?auto=format&fit=crop&w=800&q=80", Category = "Cookies", IsAvailable = true }
        );
        db.SaveChanges();
    }
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", app = "SweetBite API" }));

app.MapGet("/api/products", async (SweetBiteDbContext db) =>
    Results.Ok(await db.Products.AsNoTracking().Where(p => p.IsAvailable).OrderBy(p => p.Id).ToListAsync()));

app.MapGet("/api/products/{id:int}", async (int id, SweetBiteDbContext db) =>
{
    var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.IsAvailable);
    return product is null ? Results.NotFound(new { message = "Product not found." }) : Results.Ok(product);
});

app.MapPost("/api/orders", async (CreateOrderRequest request, SweetBiteDbContext db) =>
{
    if (request.CustomerName is null || request.CustomerName.Trim().Length < 2)
        return Results.BadRequest(new { message = "Please enter your name." });
    if (request.CustomerEmail is null || !request.CustomerEmail.Contains('@'))
        return Results.BadRequest(new { message = "Please enter a valid email address." });
    if (request.Items is null || request.Items.Count == 0)
        return Results.BadRequest(new { message = "Your cart is empty." });
    if (request.Items.Any(i => i.Quantity < 1 || i.Quantity > 50))
        return Results.BadRequest(new { message = "Each quantity must be between 1 and 50." });

    var ids = request.Items.Select(i => i.ProductId).Distinct().ToList();
    var products = await db.Products.Where(p => ids.Contains(p.Id) && p.IsAvailable).ToDictionaryAsync(p => p.Id);
    if (products.Count != ids.Count)
        return Results.BadRequest(new { message = "One or more products are unavailable." });

    var order = new CustomerOrder
    {
        CustomerName = request.CustomerName.Trim(),
        CustomerEmail = request.CustomerEmail.Trim(),
        Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
        CreatedAtUtc = DateTime.UtcNow,
        Status = "Received"
    };

    foreach (var item in request.Items)
    {
        var product = products[item.ProductId];
        order.Items.Add(new OrderItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = item.Quantity
        });
    }

    order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return Results.Created($"/api/orders/{order.Id}", new
    {
        order.Id,
        order.CustomerName,
        order.Total,
        order.Status,
        Message = "Your demo order was saved successfully."
    });
});

app.MapGet("/api/orders/{id:int}", async (int id, SweetBiteDbContext db) =>
{
    var order = await db.Orders.AsNoTracking().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
    return order is null ? Results.NotFound(new { message = "Order not found." }) : Results.Ok(order);
});

app.Run();
