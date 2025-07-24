namespace InventoryAPI.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastLogin { get; set; } = DateTimeOffset.UtcNow;
    public required string Role { get; set; } = "customer"; // Default role

}