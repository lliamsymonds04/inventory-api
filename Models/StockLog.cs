namespace InventoryAPI.Models;

public class StockLog
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int QuantityChange { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? ChangeType { get; set; } // e.g., "restock", "sale", "adjustment"
    public string? UserId { get; set; } // User who made the change
}