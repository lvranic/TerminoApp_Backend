// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UnavailableDay> UnavailableDays => Set<UnavailableDay>();
        public DbSet<Reservation> Reservations => Set<Reservation>(); // 👈 DODANO

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).IsRequired();
                e.Property(x => x.Password).IsRequired();
                e.Property(x => x.Role).IsRequired();
            });

            modelBuilder.Entity<UnavailableDay>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.AdminId).IsRequired();
                e.Property(x => x.Date).HasColumnType("date");
            });

            modelBuilder.Entity<Reservation>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.ProviderId).IsRequired();
                e.Property(x => x.ServiceId).IsRequired();
                e.Property(x => x.StartsAt).IsRequired();          // timestamp with time zone (Postgres)
                e.Property(x => x.DurationMinutes).HasDefaultValue(30);
                e.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Pending");
            });
        }
    }
}