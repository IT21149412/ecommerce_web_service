/*
 * ------------------------------------------------------------------------------
 * File:       User.cs
 * Description: 
 *             Defines the User class to represent users in the system. It includes 
 *             attributes like name, email, password, role, activation status, 
 *             creation date, and last login timestamp.
 * ------------------------------------------------------------------------------
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class User
{
    // The unique identifier for each user, represented as MongoDB's ObjectId
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    // The name of the user
    public string Name { get; set; } = string.Empty;    

    // Email of the user, represented in the MongoDB collection as "email"
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty; 

    // The hashed password of the user (should never store plain text passwords)
    public string Password { get; set; } = string.Empty;  // Store hashed passwords

    // The role of the user, could be 'Administrator', 'Vendor', or 'CSR'
    public string Role { get; set; } = "User"; // Administrator, Vendor, CSR

    // Indicates whether the user account is active or inactive, default is true (active)
    public bool IsActive { get; set; } = true;

    // Timestamp when the user account was created, default to the current UTC time
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // The last time the user logged in, nullable (can be null if the user hasn't logged in yet)
    public DateTime? LastLoginAt { get; set; }
}
