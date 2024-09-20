using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    // Create a new order
    [HttpPost("create")]
    [Authorize(Roles = "CSR, Administrator, Vendor, Customer")]
    public async Task<ActionResult> CreateOrder([FromBody] Order order)
    {
        // Extract CustomerId from the JWT token (assuming it's stored in NameIdentifier)
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(customerId))
        {
            return Unauthorized("Customer ID is not found in the token.");
        }

        // Assign the CustomerId to the order
        order.CustomerId = customerId;

        // Create the order and calculate the total price
        await _orderService.CreateOrderAsync(order);

        return Ok("Order created successfully.");
    }


    // Get order by ID
    [HttpGet("{id}")]
    [Authorize(Roles = "CSR, Administrator, Vendor, Customer")]
    public async Task<ActionResult<Order>> GetOrderById(string id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound("Order not found.");
        }
        return Ok(order);
    }

    // Cancel an order
    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "CSR, Administrator")]
    public async Task<ActionResult> CancelOrder(string id, [FromBody] CancelOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
        {
            return BadRequest("The note field is required.");
        }

        await _orderService.CancelOrderAsync(id, request.Note);
        return Ok("Order cancelled successfully.");
    }

    // Mark order as delivered
    [HttpPut("{id}/deliver")]
    [Authorize(Roles = "CSR, Administrator")]
    public async Task<ActionResult> MarkOrderAsDelivered(string id)
    {
        await _orderService.MarkOrderAsDeliveredAsync(id);
        return Ok("Order marked as delivered.");
    }

    // Mark order as partially delivered (Vendor specific)
    [HttpPut("{id}/partially-delivered/{vendorId}")]
    [Authorize(Roles = "Vendor")]
    public async Task<ActionResult> MarkOrderPartiallyDelivered(string id, string vendorId)
    {
        await _orderService.MarkOrderPartiallyDeliveredAsync(id, vendorId);
        return Ok("Order marked as partially delivered for vendor.");
    }

    // Get all orders for a customer
    [HttpGet("customer/{customerId}")]
    [Authorize(Roles = "Customer, Administrator")]
    public async Task<ActionResult<List<Order>>> GetOrdersByCustomerId(string customerId)
    {
        var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
        return Ok(orders);
    }
}
