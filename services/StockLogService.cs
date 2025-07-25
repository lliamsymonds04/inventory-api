using InventoryAPI.Data;
using InventoryAPI.Helpers;
using InventoryAPI.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

namespace InventoryAPI.Services;

public interface IStockLogService
{
    Task LogStockChangeAsync(int productId, int warehouseId,
        int quantityChange, int quantityBefore, ChangeType changeType, int? userId = null);

    Task<IEnumerable<StockLog>> GetStockLogsAsync(
        string? productName = null, string? username = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null);

    Task<IEnumerable<StockLogDto>> GetStockLogsDtosAsync(
        string? productName = null, string? username = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null);

    Task<PagedResult<StockLogDto>> GetPagedStockLogsAsync(
        int page, int pageSize,
        string? productName = null, string? username = null, int? warehouseId = null,
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

    public async Task<IEnumerable<StockLog>> GetStockLogsAsync(string? productName = null, string? username = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null)
    {
        var query = _context.StockLogs
            .Include(sl => sl.Product)
            .Include(sl => sl.Warehouse)
            .Include(sl => sl.User) 
            .AsQueryable();

        if (!string.IsNullOrEmpty(productName))
            query = query.Where(sl => sl.Product != null && sl.Product.Name != null &&
                sl.Product.Name.ToLower().Contains(productName.ToLower()));

        if (!string.IsNullOrEmpty(username))
            query = query.Where(sl => sl.User != null && sl.User.Username != null &&
                sl.User.Username.ToLower().Contains(username.ToLower()));

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

    public async Task<IEnumerable<StockLogDto>> GetStockLogsDtosAsync(string? productName = null, string? username = null,
        int? warehouseId = null, DateTime? fromDate = null, DateTime? toDate = null, ChangeType? changeType = null)
    {
        var logs = await GetStockLogsAsync(productName, username, warehouseId, fromDate, toDate, changeType);
        if (logs == null || !logs.Any())
            return Enumerable.Empty<StockLogDto>();

        return logs.Select(sl => ConvertStockLogToDto(sl)!);
    }

    public async Task<PagedResult<StockLogDto>> GetPagedStockLogsAsync(int page, int pageSize,
        string? productName = null, string? username = null, int? warehouseId = null,
        DateTime? fromDate = null, DateTime? toDate = null,
        ChangeType? changeType = null)
    {
        var logs = await GetStockLogsDtosAsync(productName, username, warehouseId, fromDate, toDate, changeType);
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