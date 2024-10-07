/*
 * ------------------------------------------------------------------------------
 * File:       CategoryService.cs
 * Description: 
 *             Provides services related to managing product categories. 
 *             It includes methods for fetching, creating, and updating categories 
 *             in the MongoDB database.
 * ------------------------------------------------------------------------------
 */

using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CategoryService
{
    private readonly IMongoCollection<Category> _categories;

    public CategoryService(MongoDbContext context)
    {
        _categories = context.Categories;
    }

    // Get all categories
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _categories.Find(category => true).ToListAsync();
    }

    // Get category by ID
    public async Task<Category?> GetCategoryByIdAsync(string id)
    {
        return await _categories.Find(category => category.Id == id).FirstOrDefaultAsync();
    }

    // Get category by name
    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        return await _categories.Find(category => category.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
    }

    // Create a new category
    public async Task CreateCategoryAsync(Category category)
    {
        await _categories.InsertOneAsync(category);
    }

    // Update an existing category
    public async Task UpdateCategoryAsync(string id, Category updatedCategory)
    {
        await _categories.ReplaceOneAsync(category => category.Id == id, updatedCategory);
    }
}
