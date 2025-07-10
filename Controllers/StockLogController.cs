using Microsoft.AspNetCore.Mvc;
using InventoryAPI.Models;
using InventoryAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockLogController : ControllerBase
{
    private readonly StockLogService _stockLogService;

    public StockLogController(StockLogService stockLogService)
    {
        _stockLogService = stockLogService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Warehouse")]
    public async Task<ActionResult<IEnumerable<StockLog>>> GetStockLogs(
        [FromQuery] int? productId = null,
        [FromQuery] int? warehouseId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? changeType = null
    )
    {
        var logs = await _stockLogService.GetStockLogsAsync(productId, warehouseId, fromDate, toDate, changeType);
        return Ok(logs);
    }
}