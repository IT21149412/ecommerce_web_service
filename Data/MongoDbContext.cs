using MongoDB.Driver;

public class MongoDbContext
{
    private readonly IMongoDatabase _db;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _db = client.GetDatabase(databaseName);
    }

    // Define collections here. Example:
    public IMongoCollection<User> Users => _db.GetCollection<User>("Users");
    public IMongoCollection<Product> Products => _db.GetCollection<Product>("Products");
    public IMongoCollection<Order> Orders => _db.GetCollection<Order>("Orders");
    public IMongoCollection<Inventory> Inventory => _db.GetCollection<Inventory>("Inventory");
}