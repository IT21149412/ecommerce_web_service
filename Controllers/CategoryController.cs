using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;
    private readonly ProductService _productService;

    public CategoryController(CategoryService categoryService, ProductService productService)
    {
        _categoryService = categoryService;
        _productService = productService;
    }

    // Create a new category (Only Admin can create categories)
    [HttpPost("create")]
    [Authorize(Roles = "Administrator")] // Only Admins can create categories
    public async Task<ActionResult> CreateCategory([FromBody] Category category)
    {
        // Check if category already exists
        var existingCategory = await _categoryService.GetCategoryByNameAsync(category.Name);
        if (existingCategory != null)
        {
            return BadRequest("Category with this name already exists.");
        }

        // Create the category
        await _categoryService.CreateCategoryAsync(category);
        return Ok("Category created successfully.");
    }

    // Deactivate a category
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Administrator")] // Only Admins can deactivate categories
    public async Task<ActionResult> DeactivateCategory(string id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        // Deactivate the category
        category.IsActive = false;
        await _categoryService.UpdateCategoryAsync(id, category);

        // Deactivate all products under this category
        await _productService.DeactivateProductsByCategoryAsync(id);

        return Ok("Category and its products deactivated successfully.");
    }

    // Activate a category
    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Administrator")] // Only Admins can activate categories
    public async Task<ActionResult> ActivateCategory(string id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        // Activate the category
        category.IsActive = true;
        await _categoryService.UpdateCategoryAsync(id, category);

        // Activate all products under this category
        await _productService.ActivateProductsByCategoryAsync(id);

        return Ok("Category and its products activated successfully.");
    }

    // Get all categories
    [HttpGet]
    public async Task<ActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }
}
