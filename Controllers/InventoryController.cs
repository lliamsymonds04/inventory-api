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
    public async Task<ActionResult<IEnumerable<Inventory>>> GetInventory([FromQuery] int? warehouseId = null)
    {
        var query = _context.Inventory.AsQueryable();
        
        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }
        
        return await query.ToListAsync();
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
        // Verify warehouse exists
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == inventory.WarehouseId);
        if (!warehouseExists)
        {
            return BadRequest($"Warehouse with ID {inventory.WarehouseId} not found.");
        }

        _context.Inventory.Add(inventory);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetInventory), new { id = inventory.ProductId }, inventory);
    }

    [HttpPost("warehouse/{warehouseId}/product/{productId}")]
    public async Task<ActionResult<Inventory>> CreateInventoryForWarehouse(int warehouseId, int productId, [FromBody] CreateInventoryRequest request)
    {
        // Verify warehouse exists
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
        if (!warehouseExists)
        {
            return BadRequest($"Warehouse with ID {warehouseId} not found.");
        }

        // Check if inventory already exists for this product in this warehouse
        var existingInventory = await _context.Inventory
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
        if (existingInventory != null)
        {
            return BadRequest($"Inventory already exists for product {productId} in warehouse {warehouseId}.");
        }

        var inventory = new Inventory
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = request.Quantity,
            MinStockLevel = request.MinStockLevel ?? 10,
            LastRestocked = DateTime.UtcNow
        };

        _context.Inventory.Add(inventory);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetInventoryByWarehouseAndProduct), 
            new { warehouseId = warehouseId, productId = productId }, inventory);
    }

    [HttpPost("warehouse/{warehouseId}/restock/{productId}")]
    public async Task<IActionResult> RestockInventoryByWarehouse(int warehouseId, int productId, [FromBody] int quantity)
    {
        // Verify warehouse exists
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
        if (!warehouseExists)
        {
            return NotFound($"Warehouse with ID {warehouseId} not found.");
        }

        var inventory = await _context.Inventory
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
            
        if (inventory == null)
        {
            return NotFound($"Inventory for product {productId} in warehouse {warehouseId} not found.");
        }

        inventory.Quantity += quantity;
        inventory.LastRestocked = DateTime.UtcNow;
        _context.Entry(inventory).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InventoryExists(productId))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpPost("{id}/deplete")]
    public async Task<IActionResult> DepleteInventory(int id, int quantity)
    {
        var inventory = await _context.Inventory.FindAsync(id);
        if (inventory == null)
        {
            return NotFound();
        }

        if (inventory.Quantity < quantity)
        {
            return BadRequest("Insufficient stock to deplete.");
        }

        inventory.Quantity -= quantity;
        _context.Entry(inventory).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InventoryExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpPost("warehouse/{warehouseId}/deplete/{productId}")]
    public async Task<IActionResult> DepleteInventoryByWarehouse(int warehouseId, int productId, [FromBody] int quantity)
    {
        // Verify warehouse exists
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
        if (!warehouseExists)
        {
            return NotFound($"Warehouse with ID {warehouseId} not found.");
        }

        var inventory = await _context.Inventory
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
            
        if (inventory == null)
        {
            return NotFound($"Inventory for product {productId} in warehouse {warehouseId} not found.");
        }

        if (inventory.Quantity < quantity)
        {
            return BadRequest("Insufficient stock to deplete.");
        }

        inventory.Quantity -= quantity;
        _context.Entry(inventory).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InventoryExists(productId))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }


    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<Inventory>>> GetLowStockItems([FromQuery] int? warehouseId = null)
    {
        var query = _context.Inventory.Where(i => i.IsLowStock);
        
        if (warehouseId.HasValue)
        {
            query = query.Where(i => i.WarehouseId == warehouseId.Value);
        }
        
        var lowStockItems = await query.ToListAsync();
        return lowStockItems;
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<ActionResult<IEnumerable<Inventory>>> GetInventoryByWarehouse(int warehouseId)
    {
        // Verify warehouse exists
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
        if (!warehouseExists)
        {
            return NotFound($"Warehouse with ID {warehouseId} not found.");
        }

        var inventory = await _context.Inventory
            .Where(i => i.WarehouseId == warehouseId)
            .ToListAsync();
            
        return inventory;
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<Inventory>>> GetInventoryByProduct(int productId)
    {
        var inventory = await _context.Inventory
            .Where(i => i.ProductId == productId)
            .ToListAsync();
            
        return inventory;
    }

    [HttpGet("warehouse/{warehouseId}/product/{productId}")]
    public async Task<ActionResult<Inventory>> GetInventoryByWarehouseAndProduct(int warehouseId, int productId)
    {
        // Verify warehouse exists
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
        if (!warehouseExists)
        {
            return NotFound($"Warehouse with ID {warehouseId} not found.");
        }

        var inventory = await _context.Inventory
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
            
        if (inventory == null)
        {
            return NotFound($"Inventory for product {productId} in warehouse {warehouseId} not found.");
        }
            
        return inventory;
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferInventory([FromBody] TransferRequest request)
    {
        // Verify both warehouses exist
        var sourceWarehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == request.SourceWarehouseId);
        var destinationWarehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == request.DestinationWarehouseId);
        
        if (!sourceWarehouseExists)
        {
            return BadRequest($"Source warehouse with ID {request.SourceWarehouseId} not found.");
        }
        if (!destinationWarehouseExists)
        {
            return BadRequest($"Destination warehouse with ID {request.DestinationWarehouseId} not found.");
        }

        // Get source inventory
        var sourceInventory = await _context.Inventory
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.SourceWarehouseId);
            
        if (sourceInventory == null)
        {
            return NotFound($"Source inventory for product {request.ProductId} in warehouse {request.SourceWarehouseId} not found.");
        }

        if (sourceInventory.Quantity < request.Quantity)
        {
            return BadRequest("Insufficient stock in source warehouse to transfer.");
        }

        // Get or create destination inventory
        var destinationInventory = await _context.Inventory
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.DestinationWarehouseId);

        if (destinationInventory == null)
        {
            // Create new inventory record for destination warehouse
            destinationInventory = new Inventory
            {
                ProductId = request.ProductId,
                WarehouseId = request.DestinationWarehouseId,
                Quantity = request.Quantity,
                MinStockLevel = sourceInventory.MinStockLevel,
                LastRestocked = DateTime.UtcNow
            };
            _context.Inventory.Add(destinationInventory);
        }
        else
        {
            // Update existing inventory
            destinationInventory.Quantity += request.Quantity;
            destinationInventory.LastRestocked = DateTime.UtcNow;
            _context.Entry(destinationInventory).State = EntityState.Modified;
        }

        // Update source inventory
        sourceInventory.Quantity -= request.Quantity;
        _context.Entry(sourceInventory).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }

        return Ok(new { 
            Message = $"Successfully transferred {request.Quantity} units of product {request.ProductId} from warehouse {request.SourceWarehouseId} to warehouse {request.DestinationWarehouseId}",
            SourceInventory = sourceInventory,
            DestinationInventory = destinationInventory
        });
    }

    private bool InventoryExists(int id)
    {
        return _context.Inventory.Any(e => e.ProductId == id);
    }
}