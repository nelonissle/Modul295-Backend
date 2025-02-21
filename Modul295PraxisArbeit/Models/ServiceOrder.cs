
using System.ComponentModel.DataAnnotations;


namespace Modul295PraxisArbeit.Models
{
    public class ServiceOrderX
    {
        [Key] // Diese Annotation definiert die Property als Primärschlüssel
        public int OrderId { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? priority { get; set; }
        public string? service { get; set; }
       // public string? Status { get; set; }
       // public int? AssignedTo { get; set; }
        public string? Status { get; set; } // Status field to track the order status
        public int? AssignedUserId { get; set; } // Nullable user ID field
        // Navigation Property
        public UsersX? AssignedUser { get; set; }
    }
}
