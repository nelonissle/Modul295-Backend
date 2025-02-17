using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class OrderUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // ✅ Richtig als string

    [BsonElement("UserId")]
    public string UserId { get; set; } = ObjectId.GenerateNewId().ToString(); // ✅ Jetzt ein string

    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
}
