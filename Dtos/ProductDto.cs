using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public required decimal Price { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Min Stock Level must be at least 1.")]
    public required int MinStockLevel { get; set; }
}