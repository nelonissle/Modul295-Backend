using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Modul295PraxisArbeit.Models
{
    public class OrderUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
