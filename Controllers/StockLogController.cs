using Microsoft.AspNetCore.Mvc;
using InventoryAPI.Models;
using InventoryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using InventoryAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockLogController : ControllerBase
{
    private readonly IStockLogService _stockLogService;
    private readonly AppDbContext _context;

    public StockLogController(AppDbContext context, IStockLogService stockLogService)
    {
        _context = context;
        _stockLogService = stockLogService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Warehouse")]
    public async Task<ActionResult<IEnumerable<StockLog>>> GetStockLogs(
        [FromQuery] int? productId = null,
        [FromQuery] int? warehouseId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] ChangeType? changeType = null
    )
    {
        var logs = await _stockLogService.GetStockLogsAsync(productId, warehouseId, fromDate, toDate, changeType);
        return Ok(logs);
    }

    [HttpGet("sales/today")]
    [Authorize(Roles = "Admin,Warehouse")]
    public async Task<int> GetSalesTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var logs = await _context.StockLogs
            .Where(sl => sl.Timestamp >= today && sl.ChangeType == ChangeType.Sale)
            .ToListAsync();

        //group by product
        var salesByProduct = logs.GroupBy(sl => sl.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalSales = g.Sum(sl => Math.Abs(sl.QuantityChange))
            });

        //get the price of each product
        var productPrices = await _context.Products
            .Where(p => salesByProduct.Select(s => s.ProductId).Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Price);

        //calculate total sales value
        var totalSalesValue = salesByProduct.Sum(s => s.TotalSales * productPrices[s.ProductId]);

        return (int)totalSalesValue;
    }
}