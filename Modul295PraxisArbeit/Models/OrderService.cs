using Modul295PraxisArbeit.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Modul295PraxisArbeitOrder.Models
{
    public class OrderService
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? OrderId { get; set; } = ObjectId.GenerateNewId().ToString();

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Priority { get; set; }
        public string? Service { get; set; }
        public string? Status { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? AssignedUserId { get; set; }
        public OrderUser? AssignedUser { get; set; }
    }
}
