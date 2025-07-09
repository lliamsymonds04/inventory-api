namespace InventoryAPI.Models;

public class CreateInventoryRequest
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public int? MinStockLevel { get; set; }
}
