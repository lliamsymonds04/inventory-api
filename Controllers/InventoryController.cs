using Microsoft.AspNetCore.Mvc;
using InventoryAPI.Data;
using InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public InventoryController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Inventory>>> GetInventory()
    {
        return await _context.Inventory.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Inventory>> GetInventory(int id)
    {
        var inventory = await _context.Inventory.FindAsync(id);
        if (inventory == null)
        {
            return NotFound();
        }
        return inventory;
    }

    [HttpPost]
    public async Task<ActionResult<Inventory>> CreateInventory(Inventory inventory)
    {
        _context.Inventory.Add(inventory);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetInventory), new { id = inventory.ProductId }, inventory);
    }


    [HttpGet("/low-stock")]
    public async Task<ActionResult<IEnumerable<Inventory>>> GetLowStockItems()
    {
        var lowStockItems = await _context.Inventory
            .Where(i => i.IsLowStock)
            .ToListAsync();
        return lowStockItems;
    }

    private bool InventoryExists(int id)
    {
        return _context.Inventory.Any(e => e.ProductId == id);
    }
}