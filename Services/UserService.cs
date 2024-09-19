using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCrypt.Net; // For password hashing

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(MongoDbContext context)
    {
        _users = context.Users;
    }

    // Get all users
    public async Task<List<User>> GetUsersAsync()
    {
        return await _users.Find(user => true).ToListAsync();
    }

    // Get a single user by ID (nullable return type)
    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
    }

    // Get user by email (for login) (nullable return type)
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
    }

    // Create a new user
    public async Task CreateUserAsync(User user)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // Hash password before storing
        await _users.InsertOneAsync(user);
    }

    // Update user information
    public async Task UpdateUserAsync(string id, User updatedUser)
    {
        await _users.ReplaceOneAsync(user => user.Id == id, updatedUser);
    }

    // Delete a user
    public async Task DeleteUserAsync(string id)
    {
        await _users.DeleteOneAsync(user => user.Id == id);
    }

    // Verify login credentials (nullable return type)
    public async Task<User> AuthenticateAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return user;
        }
        return null;
    }

}
