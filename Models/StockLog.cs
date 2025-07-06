namespace InventoryAPI.Models;

public class StockLog
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int InventoryId { get; set; }
    public int QuantityChange { get; set; }
    public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
    public string? ChangeType { get; set; } // e.g., "restock", "sale", "adjustment"
    
    // Navigation property
    public Inventory? Inventory { get; set; }
}