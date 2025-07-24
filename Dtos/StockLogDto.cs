using System.Text.Json.Serialization;

namespace InventoryAPI.Dtos;

public class StockLogDto
{
    public int Id { get; set; }
    public required int ProductId { get; set; }
    public required int WarehouseId { get; set; }
    public required int QuantityChange { get; set; }
    public required int QuantityBefore { get; set; }
    public required int QuantityAfter { get; set; }
    public required DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Username { get; set; } // Username of the user who made the
    public string? ProductName { get; set; } // Name of the product

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ChangeType ChangeType { get; set; }
}

