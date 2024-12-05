
using System.ComponentModel.DataAnnotations;


namespace Modul295PraxisArbeit.Models
{
    public class ServiceOrder
    {
        [Key] // Diese Annotation definiert die Property als Primärschlüssel
        public int OrderId { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Priority { get; set; }
        public string? Service { get; set; }
        public string? Status { get; set; }
        public int? AssignedTo { get; set; }

        // Navigation Property
        public User? AssignedUser { get; set; }
    }
}
