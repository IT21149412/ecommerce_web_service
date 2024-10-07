/*
 * ------------------------------------------------------------------------------
 * File:       VendorReview.cs
 * Description: 
 *             Defines the VendorReview class to represent reviews left by customers 
 *             for vendors. It includes the vendor ID, customer ID, comment, rating, 
 *             and a creation timestamp.
 * ------------------------------------------------------------------------------
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class VendorReview
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string VendorId { get; set; } = string.Empty;  // Vendor's ID
    public string CustomerId { get; set; } = string.Empty;  // Customer's ID
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }  // Rating between 1-5
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class UpdateCommentModel
{
    public string NewComment { get; set; } = string.Empty;  // This is the new comment field
}

