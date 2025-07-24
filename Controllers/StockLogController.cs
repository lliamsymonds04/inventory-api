using Microsoft.AspNetCore.Mvc;
using InventoryAPI.Models;
using InventoryAPI.Dtos;
using InventoryAPI.Services;
using InventoryAPI.Helpers;
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

    [HttpGet("sales/today")]
    [Authorize(Roles = "Admin,Warehouse")]
    public async Task<int> GetSalesToday()
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


    [HttpGet]
    [Authorize(Roles = "Admin,Warehouse")]
    public async Task<ActionResult<PagedResult<StockLogDto>>> GetStockLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] int? productId = null, [FromQuery] int? warehouseId = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null,
        [FromQuery] ChangeType? changeType = null)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Page and PageSize must be greater than 0.");
        }
        var pagedResult = await _stockLogService.GetPagedStockLogsAsync(page, pageSize, productId, warehouseId, fromDate, toDate, changeType);
        return Ok(pagedResult);

    }
}