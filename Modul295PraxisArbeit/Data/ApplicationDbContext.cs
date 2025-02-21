using Microsoft.EntityFrameworkCore;
using Modul295PraxisArbeit.Models; // Importiere deine Models

namespace Modul295PraxisArbeit.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet für jede Tabelle
        public DbSet<OrderUser> Users { get; set; }
        public DbSet<OrderService> OrderServices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMongoDB("mongodb://localhost:27017", "MyDatabase");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Weitere Konfigurationen (z. B. Beziehungen)
        }
    }
}
