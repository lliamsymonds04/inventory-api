using InventoryAPI.Data;
using InventoryAPI.Models;


public interface IProductService
{
    Task<Product> CreateProductAsync(Product product);
    Task<Inventory> AddInventoryAsync(int productId, int warehouseId, int quantity = 0, int minStockLevel = 0);
}

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Inventory> AddInventoryAsync(int productId, int warehouseId, int quantity = 0, int minStockLevel = 0)
    {
        var inventory = new Inventory
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            MinStockLevel = minStockLevel
        };

        _context.Inventory.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }
}