namespace InventoryAPI.Models;

public class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public int MinStockLevel { get; set; } = 10;
    public bool IsLowStock => Quantity < MinStockLevel;
    public DateTimeOffset LastRestocked { get; set; } = DateTimeOffset.UtcNow;
}