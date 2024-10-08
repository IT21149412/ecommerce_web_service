/*
 * ------------------------------------------------------------------------------
 * File:       Order.cs
 * Description: 
 *             Defines the Order class to represent customer orders. It includes 
 *             details like the customer ID, order items, status, created date, 
 *             and vendor statuses. Each order contains a list of order items 
 *             and total price.
 * ------------------------------------------------------------------------------
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public string OrderStatus { get; set; } = "Purchased";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Note { get; set; } = string.Empty;
    public bool IsPartiallyDelivered { get; set; } = false;

    public List<VendorOrderStatus> VendorStatuses { get; set; } = new List<VendorOrderStatus>();

    // Add TotalOrderPrice field
    public decimal TotalOrderPrice { get; set; }  // Total price for the order
    public string VendorId { get; set; } = string.Empty;
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; }  // Price for one unit of the product
    public decimal TotalPrice { get; set; }  // Total price for this item (Price * Quantity)
    public string VendorId { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string Status { get; set; } = "Purchased";

}


public class VendorOrderStatus
{
    public string VendorId { get; set; } = string.Empty;
    public bool IsDelivered { get; set; } = false;  // Marked by the vendor
}

public class CancelOrderRequest
{
    public string Note { get; set; } = string.Empty; // Ensuring the note is not null by default
}

