using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<UnavailableDay> UnavailableDays { get; set; } = default!;

        public DbSet<User> Users { get; set; }

        // Tu dodaj kasnije i ostale modele (Appointments, Reservations, itd.)
    }
}