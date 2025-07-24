using System.Text.Json.Serialization;
using InventoryAPI.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChangeType
{
    InitialStock,
    Restock,
    Sale,
    TransferIn,
    TransferOut,
}

public class StockLog
{
    public int Id { get; set; }
    public required int ProductId { get; set; }
    public required int WarehouseId { get; set; }
    public required int QuantityChange { get; set; }
    public required int QuantityBefore { get; set; }
    public required int QuantityAfter { get; set; }
    public required DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public required ChangeType ChangeType { get; set; }
    public int? UserId { get; set; } // User who made the change
    public User? User { get; set; } // Navigation property for User
    public Product Product { get; set; } = null!; // Navigation property for Product
    public Warehouse Warehouse { get; set; } = null!; // Navigation property for Warehouse
}