using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChangeType
{
    InitialStock,
    Restock,
    Sale,
    TransferIn,
    TransferOut,
}

public class StockLog
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int QuantityChange { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ChangeType ChangeType { get; set; }
    public string? UserId { get; set; } // User who made the change
}