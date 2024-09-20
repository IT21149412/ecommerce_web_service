using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; } = true;

    public string CategoryId { get; set; }

    public string VendorId { get; set; } = string.Empty; // Vendor's Id who created the product

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Inventory-related fields
    public bool IsLowStock => Stock < 5;  // Flag if low stock
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

}
