
using System.ComponentModel.DataAnnotations;


namespace Modul295PraxisArbeit.Models
{
    public class ServiceOrder
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

        // Navigation Property
        public User? AssignedUser { get; set; }
    }
}
