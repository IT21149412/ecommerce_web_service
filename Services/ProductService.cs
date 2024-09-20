using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProductService
{
    private readonly IMongoCollection<Product> _products;
    private readonly OrderService _orderService; // Declare OrderService

    public ProductService(MongoDbContext context, OrderService orderService) // Inject OrderService properly
    {
        _products = context.Products;
        _orderService = orderService;  // Initialize the OrderService
    }

    // Get all products (Admin/CSR can view all)
    public async Task<List<Product>> GetProductsAsync()
    {
        return await _products.Find(product => true).ToListAsync();
    }

    // Get product by ID
    public async Task<Product?> GetProductByIdAsync(string id)
    {
        return await _products.Find(product => product.Id == id).FirstOrDefaultAsync();
    }

    // Create a new product (Vendor only)
    public async Task CreateProductAsync(Product product)
    {
        await _products.InsertOneAsync(product);
    }

    // Update product (Vendor only)
    public async Task UpdateProductAsync(string id, Product updatedProduct)
    {
        var existingProduct = await _products.Find(product => product.Id == id).FirstOrDefaultAsync();
        if (existingProduct != null)
        {
            var update = Builders<Product>.Update
                .Set(p => p.Name, updatedProduct.Name)
                .Set(p => p.Description, updatedProduct.Description)
                .Set(p => p.Price, updatedProduct.Price)
                .Set(p => p.Stock, updatedProduct.Stock)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            await _products.UpdateOneAsync(product => product.Id == id, update);
        }
    }

    // Activate product (Vendor/Admin only)
    public async Task ActivateProductAsync(string id)
    {
        var update = Builders<Product>.Update.Set(p => p.IsActive, true);
        await _products.UpdateOneAsync(product => product.Id == id, update);
    }

    // Deactivate product (Vendor/Admin only)
    public async Task DeactivateProductAsync(string id)
    {
        var update = Builders<Product>.Update.Set(p => p.IsActive, false);
        await _products.UpdateOneAsync(product => product.Id == id, update);
    }

    // Deactivate all products under a specific category
    public async Task DeactivateProductsByCategoryAsync(string categoryId)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId);
        var update = Builders<Product>.Update.Set(p => p.IsActive, false); // Deactivate all products
        await _products.UpdateManyAsync(filter, update);
    }

    // Activate all products under a specific category
    public async Task ActivateProductsByCategoryAsync(string categoryId)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId);
        var update = Builders<Product>.Update.Set(p => p.IsActive, true); // Activate all products
        await _products.UpdateManyAsync(filter, update);
    }

    //inventory related services
    // Update the stock of a product
    public async Task UpdateStockAsync(string productId, int newStock)
    {
        var update = Builders<Product>.Update
            .Set(p => p.Stock, newStock)  // Update the Stock field
            .Set(p => p.LastUpdated, DateTime.UtcNow);

        await _products.UpdateOneAsync(p => p.Id == productId, update);
    }

    // Check if the product is in low stock
    public async Task<bool> IsLowStockAsync(string productId)
    {
        var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
        return product != null && product.IsLowStock;
    }

    // Delete a product (ensure no pending orders exist)
    public async Task DeleteProductAsync(string productId)
    {
        // Check if the product is part of any pending orders
        var hasPendingOrders = await _orderService.HasPendingOrdersForProductAsync(productId);

        if (hasPendingOrders)
        {
            throw new InvalidOperationException("Cannot delete product because it is part of a pending order.");
        }

        // If no pending orders, proceed with deletion
        await _products.DeleteOneAsync(p => p.Id == productId);
    }
}
