using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using InventoryAPI.Data;
using InventoryAPI.Models; 

public static class SeedData
{

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        await CreateAdminUser(serviceProvider);
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

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
