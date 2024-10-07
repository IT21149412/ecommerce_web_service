/*
 * ------------------------------------------------------------------------------
 * File:       InventoryController.cs
 * Description: 
 *             This file defines the InventoryController class, which provides API 
 *             endpoints for managing and monitoring product inventory. It allows 
 *             authorized users (Administrators and Vendors) to check stock levels, 
 *             update stock levels, and send notifications for low stock products.
 * ------------------------------------------------------------------------------
 */

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly NotificationService _notificationService;

    public InventoryController(ProductService productService,NotificationService notificationService)
    {
        _productService = productService;
        _notificationService = notificationService;

    }

    // Check stock level
    [HttpGet("check/{productId}")]
    [Authorize(Roles = "Administrator")] // Only Admins can check stock levels
    public async Task<IActionResult> CheckStock(string productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return NotFound("Product not found.");
        }
        return Ok(new { Stock = product.Stock });  
    }

    // Update stock level
    [HttpPut("update/{productId}")]
    [Authorize(Roles = "Vendor, Administrator")] // Only Admins can update stock
    public async Task<IActionResult> UpdateStock(string productId, [FromBody] int newStock)
    {
        await _productService.UpdateStockAsync(productId, newStock);
        await NotifyLowStock(productId);
        return Ok("Stock updated successfully.");
    }

    // Notify if low stock
    // [HttpPost("notify-low-stock/{productId}")]
    // [Authorize(Roles = "Administrator")] // Only Admins can send low-stock notifications
    // public async Task<IActionResult> NotifyLowStock(string productId)
    // {
    //     var isLowStock = await _productService.IsLowStockAsync(productId);
    //     if (isLowStock)
    //     {
    //         // Notify the vendor (assuming there is a notification service)
    //         return Ok("Low stock notification sent.");
    //     }
    //     return Ok("Stock level is sufficient.");
    // }

    [HttpPost("notify-low-stock/{productId}")]
    [Authorize(Roles = "Administrator")] // Only Admins can send low-stock notifications
    public async Task<IActionResult> NotifyLowStock(string productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null) return NotFound("Product not found.");

        var isLowStock = await _productService.IsLowStockAsync(productId);
        if (isLowStock)
        {
            // Create a notification entry for the vendor
            var notification = new Notification
            {
                VendorId = product.VendorId,
                Message = $"Low stock alert for product {product.Name} ({product.Stock} units left).",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            // Save the notification to the database
            await _notificationService.SaveNotificationAsync(notification);

            return Ok("Low stock notification saved.");
        }

        return Ok("Stock level is sufficient.");
    }

}
