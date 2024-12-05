using Microsoft.EntityFrameworkCore;
using Modul295PraxisArbeit.Models; // Importiere deine Models

namespace Modul295PraxisArbeit.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet für jede Tabelle
        public DbSet<User> Users { get; set; }
        public DbSet<ServiceOrder> ServiceOrders { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: Initiale Daten hinzufügen (Seed-Daten)
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "admin", PasswordHash = "hashedPassword", Role = "Admin" }
            );

            // Weitere Konfigurationen (z. B. Beziehungen)
        }
    }
}
