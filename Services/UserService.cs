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

    // Update user information (partial update)
    public async Task UpdateUserAsync(string id, User updatedUser)
    {
        var existingUser = await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            var updateDefinitionBuilder = Builders<User>.Update;
            var updateDefinition = new List<UpdateDefinition<User>>();

            // Only update fields that are provided in the request body
            if (!string.IsNullOrEmpty(updatedUser.Name))
            {
                updateDefinition.Add(updateDefinitionBuilder.Set(u => u.Name, updatedUser.Name));
            }
            
            if (!string.IsNullOrEmpty(updatedUser.Email))
            {
                updateDefinition.Add(updateDefinitionBuilder.Set(u => u.Email, updatedUser.Email));
            }
            
            if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
            {
                updateDefinition.Add(updateDefinitionBuilder.Set(u => u.PasswordHash, updatedUser.PasswordHash));
            }
            
            if (!string.IsNullOrEmpty(updatedUser.Role))
            {
                updateDefinition.Add(updateDefinitionBuilder.Set(u => u.Role, updatedUser.Role));
            }

            // Handle isActive field explicitly, as it's a boolean (no need for string check)
            updateDefinition.Add(updateDefinitionBuilder.Set(u => u.IsActive, updatedUser.IsActive));

            // If there are updates to apply
            if (updateDefinition.Count > 0)
            {
                var update = updateDefinitionBuilder.Combine(updateDefinition);
                await _users.UpdateOneAsync(user => user.Id == id, update);
            }
        }
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
