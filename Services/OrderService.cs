/*
 * ------------------------------------------------------------------------------
 * File:       OrderService.cs
 * Description: 
 *             Provides services for managing customer orders in the system. 
 *             This includes creating orders, updating order statuses, 
 *             checking for stock availability, and managing vendor-specific deliveries.
 * ------------------------------------------------------------------------------
 */

using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderService
{
    private readonly IMongoCollection<Order> _orders;
    private readonly IMongoCollection<Product> _products;  // Add Product collection

    public OrderService(MongoDbContext context)
    {
        _orders = context.Orders;
        _products = context.Products;  // Initialize Product collection
    }

    // Create a new order
    public async Task CreateOrderAsync(Order order)
    {
        decimal totalOrderPrice = 0;

        // Loop through all items to get the product price from the Product collection
        foreach (var item in order.Items)
        {
            // Fetch the product by product ID
            var product = await _products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {item.ProductId} not found.");
            }

            // Ensure there's enough stock
            if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"Not enough stock for product {product.Name}.");
            }

            // Set the price for each item
            item.Price = product.Price;

            // Calculate the total price for this item
            item.TotalPrice = item.Price * item.Quantity;

            // Decrease the stock of the product
            var newStock = product.Stock - item.Quantity;

            // Update the product's stock in the database
            var update = Builders<Product>.Update.Set(p => p.Stock, newStock);
            await _products.UpdateOneAsync(p => p.Id == product.Id, update);

            // Add to total order price
            totalOrderPrice += item.TotalPrice;
        }

        // Set the total order price
        order.TotalOrderPrice = totalOrderPrice;

        // Insert the order into the Orders collection
        await _orders.InsertOneAsync(order);
    }


    // Update order status or details
    public async Task UpdateOrderAsync(string id, Order updatedOrder)
    {
        await _orders.ReplaceOneAsync(order => order.Id == id, updatedOrder);
    }

    // Get an order by ID
    public async Task<Order?> GetOrderByIdAsync(string id)
    {
        return await _orders.Find(order => order.Id == id).FirstOrDefaultAsync();
    }

    // Cancel order
    public async Task CancelOrderAsync(string id, string note)
    {
        var order = await GetOrderByIdAsync(id);
        if (order != null && order.Status == "Processing")
        {
            order.Status = "Cancelled";
            order.Note = note;
            await UpdateOrderAsync(id, order);
        }
    }

    // Mark order as delivered (for admin/CSR)
    public async Task MarkOrderAsDeliveredAsync(string id)
    {
        var order = await GetOrderByIdAsync(id);
        if (order != null)
        {
            order.Status = "Delivered";
            await UpdateOrderAsync(id, order);
        }
    }

    // Vendor can mark their products as partially delivered
    public async Task MarkOrderPartiallyDeliveredAsync(string id, string vendorId)
    {
        var order = await GetOrderByIdAsync(id);
        if (order != null && order.Status == "Processing")
        {
            var vendorStatus = order.VendorStatuses.Find(v => v.VendorId == vendorId);
            if (vendorStatus != null)
            {
                vendorStatus.IsDelivered = true;
            }

            if (order.VendorStatuses.TrueForAll(v => v.IsDelivered))
            {
                order.Status = "Delivered";
            }
            else
            {
                order.IsPartiallyDelivered = true;
            }

            await UpdateOrderAsync(id, order);
        }
    }

    // Get all orders by customer
    public async Task<List<Order>> GetOrdersByCustomerIdAsync(string customerId)
    {
        return await _orders.Find(order => order.CustomerId == customerId).ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByVendorIdAsync(string vendorId)
    {
        return await _orders.Find(order => order.VendorId == vendorId).ToListAsync();  
    }


    // Check if there are any pending orders containing the given product
    public async Task<bool> HasPendingOrdersForProductAsync(string productId)
    {
        // Check if any order has the product in "Processing" or other non-final statuses
        var count = await _orders.CountDocumentsAsync(order => 
            order.Items.Any(item => item.ProductId == productId) && 
            (order.Status == "Processing" || order.Status == "Partially Delivered"));

        return count > 0;
    }
}
