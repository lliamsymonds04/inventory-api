using Microsoft.AspNetCore.Identity;
using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public static class SeedData
{

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        await CreateAdminUser(serviceProvider);
        await CreateBaseWarehouse(serviceProvider);
        await CreateBaseProducts(serviceProvider);
        await CreateBaseInventory(serviceProvider);
    }

    private static async Task CreateAdminUser(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var authService = serviceProvider.GetRequiredService<IAuthService>();
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        if (await authService.UserExistsAsync("admin"))
        {
            return; // Admin user already exists
        }

        //get the password from the config 
        var adminPassword = config["AdminUser:Password"];
        if (string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException("Admin user password is not set in configuration.");
        }

        var passwordHasher = new PasswordHasher<string>();
        var hashedPassword = passwordHasher.HashPassword("admin", adminPassword);
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = hashedPassword,
            Role = "Admin",
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };

        Console.WriteLine("==/ Created admin user");

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }

    private static async Task CreateBaseWarehouse(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.Warehouses.AnyAsync())
        {
            return; // Base warehouse already exists
        }

        var baseWarehouse = new Warehouse
        {
            Name = "Base Warehouse",
            Location = "Default Location",
        };

        Console.WriteLine("==/ Created base warehouse");

        context.Warehouses.Add(baseWarehouse);
        await context.SaveChangesAsync();

        Console.WriteLine($"==/ Base warehouse created with ID: {baseWarehouse.Id}");
    }

    private static async Task CreateBaseProducts(IServiceProvider serviceProvider)
    {
        //load the json file with the products
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        if (await context.Products.AnyAsync())
        {
            return; // Base products already exist
        }

        var productsJson = await File.ReadAllTextAsync("Data/products.json");
        var products = JsonSerializer.Deserialize<List<Product>>(productsJson);

        if (products == null || !products.Any())
        {
            throw new InvalidOperationException("No products found in SeedProducts.json");
        }

        foreach (var product in products)
        {
            context.Products.Add(product);
        }
        Console.WriteLine("==/ Created base products");
        await context.SaveChangesAsync();
    }

    private static async Task CreateBaseInventory(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        if (await context.Inventory.AnyAsync())
        {
            return; // Base inventory already exists
        }

        var stockLogService = serviceProvider.GetRequiredService<IStockLogService>();

        var baseWarehouse = await context.Warehouses.FirstOrDefaultAsync();
        if (baseWarehouse == null)
        {
            throw new InvalidOperationException("Base warehouse does not exist.");
        }

        var products = await context.Products.ToListAsync();
        foreach (var product in products)
        {
            var inventoryItem = new Inventory
            {
                ProductId = product.Id,
                WarehouseId = baseWarehouse.Id,
                Quantity = 100, 
                MinStockLevel = 10,
            };
            context.Inventory.Add(inventoryItem);
            stockLogService.LogStockChangeAsync(
                product.Id, 
                baseWarehouse.Id, 
                inventoryItem.Quantity, 
                0, 
                ChangeType.InitialStock
            ).GetAwaiter().GetResult(); // Log initial stock change synchronously for seeding
        }
        
        Console.WriteLine("==/ Created base inventory");
        await context.SaveChangesAsync();
    }
}
