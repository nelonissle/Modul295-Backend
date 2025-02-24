using MongoDB.Driver;
using Modul295PraxisArbeit.Models;

namespace Modul295PraxisArbeit.Data
{
   public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string cannot be null or empty.");
        if (string.IsNullOrEmpty(databaseName))
            throw new ArgumentNullException(nameof(databaseName), "MongoDB database name cannot be null or empty.");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<OrderService> OrderServices => _database.GetCollection<OrderService>("OrderServices");
}

}
