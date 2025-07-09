namespace InventoryAPI.Models;

public class TransferRequest
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int SourceWarehouseId { get; set; }
    public int DestinationWarehouseId { get; set; }
    public int Quantity { get; set; }
}
