using InventoryAPI.Data;
using InventoryAPI.Helpers;
using InventoryAPI.Dtos;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Services;

public interface IStockLogService
{
    Task LogStockChangeAsync(int productId, int warehouseId,
        int quantityChange, int quantityBefore, ChangeType changeType, int? userId = null);

    Task<IEnumerable<StockLog>> GetStockLogsAsync(int? productId = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null);

    Task<IEnumerable<StockLogDto>> GetStockLogsDtosAsync(int? productId = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null);

    Task<PagedResult<StockLogDto>> GetPagedStockLogsAsync(int page, int pageSize,
        int? productId = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null,
        ChangeType? changeType = null);
}

public class StockLogService : IStockLogService
{
    private readonly AppDbContext _context;

    public StockLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogStockChangeAsync(int productId, int warehouseId,
        int quantityChange, int quantityBefore, ChangeType changeType, int? userId = null)
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
            Timestamp = DateTimeOffset.UtcNow
        };

        _context.StockLogs.Add(stockLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<StockLog>> GetStockLogsAsync(int? productId = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null)
    {
        var query = _context.StockLogs
            .Include(sl => sl.Product)
            .Include(sl => sl.Warehouse)
            .Include(sl => sl.User) 
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

    public async Task<IEnumerable<StockLogDto>> GetStockLogsDtosAsync(int? productId = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null)
    {
        var logs = await GetStockLogsAsync(productId, warehouseId, fromDate, toDate, changeType);
        if (logs == null || !logs.Any())
            return Enumerable.Empty<StockLogDto>();

        return logs.Select(sl => ConvertStockLogToDto(sl)!);
    }

    public async Task<PagedResult<StockLogDto>> GetPagedStockLogsAsync(int page, int pageSize,
        int? productId = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null,
        ChangeType? changeType = null)
    {
        var logs = await GetStockLogsDtosAsync(productId, warehouseId, fromDate, toDate, changeType);
        var totalCount = logs.Count();

        var items = logs
            .OrderByDescending(sl => sl.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<StockLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private StockLogDto? ConvertStockLogToDto(StockLog stockLog)
    {
        if (stockLog == null) return null;

        return new StockLogDto
        {
            Id = stockLog.Id,
            ProductId = stockLog.ProductId,
            WarehouseId = stockLog.WarehouseId,
            QuantityChange = stockLog.QuantityChange,
            QuantityBefore = stockLog.QuantityBefore,
            QuantityAfter = stockLog.QuantityAfter,
            Timestamp = stockLog.Timestamp,
            Username = stockLog.User?.Username,
            ProductName = stockLog.Product?.Name,
            ChangeType = stockLog.ChangeType
        };
    }
}