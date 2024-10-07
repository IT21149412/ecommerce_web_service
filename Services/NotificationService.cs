using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NotificationService
{
    private readonly IMongoCollection<Notification> _notifications;

    // Inject the MongoDbContext rather than IMongoClient for consistency
    public NotificationService(MongoDbContext context)
    {
        _notifications = context.Notifications;  // Using MongoDbContext to get the Notifications collection
    }

    // Save a notification to the database
    public async Task SaveNotificationAsync(Notification notification)
    {
        await _notifications.InsertOneAsync(notification);
    }

    // Get unread notifications for a vendor
    public async Task<List<Notification>> GetUnreadNotificationsAsync(string vendorId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.VendorId, vendorId) &
                     Builders<Notification>.Filter.Eq(n => n.IsRead, false);

        return await _notifications.Find(filter).ToListAsync();
    }

    // Mark a notification as read
    public async Task MarkAsReadAsync(string notificationId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _notifications.UpdateOneAsync(filter, update);
    }

    // Delete a notification
    public async Task DeleteNotificationAsync(string notificationId)
    {
        await _notifications.DeleteOneAsync(n => n.Id == notificationId);
    }

    // Get notifications by VendorId
    public async Task<List<Notification>> GetNotificationsByVendorIdAsync(string vendorId)
    {
        return await _notifications.Find(n => n.VendorId == vendorId).ToListAsync();
    }

    // Mark all notifications as read for a vendor
    public async Task MarkAllNotificationsAsReadAsync(string vendorId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.VendorId, vendorId) &
                     Builders<Notification>.Filter.Eq(n => n.IsRead, false);
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _notifications.UpdateManyAsync(filter, update);
    }

    // Get the count of unread notifications for a vendor
    public async Task<int> GetUnreadNotificationCountAsync(string vendorId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.VendorId, vendorId) &
                     Builders<Notification>.Filter.Eq(n => n.IsRead, false);
        return (int)await _notifications.CountDocumentsAsync(filter);
    }
}
