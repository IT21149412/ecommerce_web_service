using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();  

    public string VendorId { get; set; } = string.Empty;  
    public string Message { get; set; } = string.Empty;    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
    public bool IsRead { get; set; } = false;  
}
