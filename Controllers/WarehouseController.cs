using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;
using InventoryAPI.Data;
using Microsoft.AspNetCore.Mvc;

public class WarehouseController : ControllerBase
{
    private readonly AppDbContext _context;

    public WarehouseController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Warehouse>>> GetWarehouses()
    {
        return await _context.Warehouses.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Warehouse>> GetWarehouse(int id)
    {
        var warehouse = await _context.Warehouses.FindAsync(id);
        if (warehouse == null)
        {
            return NotFound();
        }
        return warehouse;
    }

    [HttpPost]
    public async Task<ActionResult<Warehouse>> CreateWarehouse(Warehouse warehouse)
    {
        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetWarehouse), new { id = warehouse.Id }, warehouse);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWarehouse(int id, Warehouse warehouse)
    {
        if (id != warehouse.Id)
        {
            return BadRequest();
        }

        _context.Entry(warehouse).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WarehouseExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWarehouse(int id)
    {
        var warehouse = await _context.Warehouses.FindAsync(id);
        if (warehouse == null)
        {
            return NotFound();
        }

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool WarehouseExists(int id)
    {
        return _context.Warehouses.Any(e => e.Id == id);
    }
}