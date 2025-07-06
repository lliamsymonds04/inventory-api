namespace InventoryAPI.Models;

public class Warehouse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? ContactNumber { get; set; }

    // Navigation property for related inventory
    public ICollection<Inventory>? Inventory { get; set; }
}