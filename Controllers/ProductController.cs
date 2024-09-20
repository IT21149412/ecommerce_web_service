using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // For ClaimTypes and FindFirstValue

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService) // Correct the constructor
    {
        _productService = productService;
    }

    // GET: api/product
    [HttpGet]
    [AllowAnonymous] // Allow anyone to view all products
    public async Task<ActionResult> GetProducts()
    {
        var products = await _productService.GetProductsAsync();
        return Ok(products);
    }

    // GET: api/product/{id}
    [HttpGet("{id}")]
    [AllowAnonymous] // Allow anyone to view individual product details
    public async Task<ActionResult<Product>> GetProductById(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    // POST: api/product/create
    [HttpPost("create")]
    [Authorize(Roles = "Vendor")] // Only Vendors can create products
    public async Task<ActionResult> CreateProduct([FromBody] Product product)
    {
        // Extract VendorId from the JWT token
        var vendorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(vendorId))
        {
            return Unauthorized("Vendor ID is not found in the token.");
        }

        // Assign the VendorId to the product
        product.VendorId = vendorId;

        // Call the service to create the product
        await _productService.CreateProductAsync(product);
        return Ok("Product created successfully");
    }

    // PUT: api/product/{id}/update
    [HttpPut("{id}/update")]
    [Authorize(Roles = "Vendor,Administrator")] // Vendors or Admins can update products
    public async Task<ActionResult> UpdateProduct(string id, [FromBody] Product updatedProduct)
    {
        await _productService.UpdateProductAsync(id, updatedProduct);
        return Ok("Product updated successfully");
    }

    // DELETE: api/product/{id}/delete
    [HttpDelete("{id}")]
    [Authorize(Roles = "Vendor, Administrator")]
    public async Task<ActionResult> DeleteProduct(string id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return Ok("Product deleted successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); // Return error if product is part of a pending order
        }
    }

    // PUT: api/product/{id}/activate
    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Vendor,Administrator")] // Vendors or Admins can activate products
    public async Task<ActionResult> ActivateProduct(string id)
    {
        await _productService.ActivateProductAsync(id);
        return Ok("Product activated successfully");
    }

    // PUT: api/product/{id}/deactivate
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Vendor,Administrator")] // Vendors or Admins can deactivate products
    public async Task<ActionResult> DeactivateProduct(string id)
    {
        await _productService.DeactivateProductAsync(id);
        return Ok("Product deactivated successfully");
    }
}
