/*
 * ------------------------------------------------------------------------------
 * File:       Category.cs
 * Description: 
 *             Defines the Category class for representing product categories in 
 *             the system. Each category has an ID, a name, an activation status, 
 *             and a creation timestamp. It is used to categorize products in the 
 *             product catalog.
 * ------------------------------------------------------------------------------
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
