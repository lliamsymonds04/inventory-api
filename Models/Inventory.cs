namespace InventoryAPI.Models;

public class Inventory
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public int MinStockLevel { get; set; } = 10;
    public bool IsLowStock => Quantity < MinStockLevel;
    public DateTime LastRestocked { get; set; } = DateTime.UtcNow;
}