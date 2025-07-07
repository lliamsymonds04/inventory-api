namespace InventoryAPI.Models;

public class CreateInventoryRequest
{
    public int Quantity { get; set; }
    public int? MinStockLevel { get; set; }
}
