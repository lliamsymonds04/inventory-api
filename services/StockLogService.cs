using InventoryAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Services;

public interface IStockLogService
{
    Task LogStockChangeAsync(int productId, int warehouseId,
        int quantityChange, int quantityBefore, ChangeType changeType, string userId);

    Task<IEnumerable<StockLog>> GetStockLogsAsync(int? productId = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null);
}

public class StockLogService : IStockLogService
{
    private readonly AppDbContext _context;

    public StockLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogStockChangeAsync(int productId, int warehouseId,
        int quantityChange, int quantityBefore, ChangeType changeType, string userId)
    {
        var stockLog = new StockLog
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            QuantityChange = quantityChange,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityBefore + quantityChange,
            UserId = userId,
            ChangeType = changeType,
            Timestamp = DateTime.UtcNow
        };

        _context.StockLogs.Add(stockLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<StockLog>> GetStockLogsAsync(int? productId = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null)
    {
        var query = _context.StockLogs
            .Include(sl => sl.ProductId)
            .Include(sl => sl.WarehouseId)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(sl => sl.ProductId == productId.Value);

        if (warehouseId.HasValue)
            query = query.Where(sl => sl.WarehouseId == warehouseId.Value);

        if (fromDate.HasValue)
            query = query.Where(sl => sl.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(sl => sl.Timestamp <= toDate.Value);

        if (changeType.HasValue)
        {
            query = query.Where(sl => sl.ChangeType == changeType.Value);
        }

        return await query.ToListAsync();
    }
}