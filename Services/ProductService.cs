using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProductService
{
    private readonly IMongoCollection<Product> _products;

    public ProductService(MongoDbContext context)
    {
        _products = context.Products;
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

    // Delete product (Vendor only)
    public async Task DeleteProductAsync(string id)
    {
        await _products.DeleteOneAsync(product => product.Id == id);
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
}
