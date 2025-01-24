using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Modul295PraxisArbeitOrder.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
