using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using BCrypt.Net;
using Modul295PraxisArbeit.Models;

public class UserService : IUserService
{
    private readonly IMongoCollection<OrderUser> _usersCollection; // ✅ Use OrderUser model

    public UserService(IMongoDatabase database)
    {
        _usersCollection = database.GetCollection<OrderUser>("Users"); // ✅ Ensure the collection name is correct
    }

    // ✅ Check if a user already exists in the database
    public async Task<bool> UserExists(string username)
    {
        return await _usersCollection.Find(u => u.Username == username).AnyAsync();
    }

    // ✅ Create a new user and hash the password
    public async Task CreateUser(string username, string password, string role)
    {
        var user = new OrderUser
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), // ✅ Secure password hashing
            Role = role
        };

        await _usersCollection.InsertOneAsync(user);
    }

    // ✅ Retrieve a user by their username
    public async Task<OrderUser> GetUserByUsername(string username)
    {
        return await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    // ✅ Update a user's role (e.g., promote to Admin)
    public async Task UpdateUserRole(string username, string role)
    {
        var filter = Builders<OrderUser>.Filter.Eq(u => u.Username, username);
        var update = Builders<OrderUser>.Update.Set(u => u.Role, role);
        await _usersCollection.UpdateOneAsync(filter, update);
    }
}
