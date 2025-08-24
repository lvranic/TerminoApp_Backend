using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<UnavailableDay> UnavailableDays => Set<UnavailableDay>();
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).IsRequired();
                e.Property(x => x.Password).IsRequired();
                e.Property(x => x.Role).IsRequired();
            });

            // Service
            modelBuilder.Entity<Service>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired();
                e.Property(x => x.DurationMinutes).HasDefaultValue(30);

                e.HasOne(s => s.Provider)
                    .WithMany(u => u.Services)
                    .HasForeignKey(s => s.ProviderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Reservation
            modelBuilder.Entity<Reservation>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.ProviderId).IsRequired();
                e.Property(x => x.ServiceId).IsRequired();
                e.Property(x => x.StartsAt).IsRequired();
                e.Property(x => x.DurationMinutes).HasDefaultValue(30);
                e.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Pending");

                e.HasOne(r => r.User)
                    .WithMany(u => u.MyReservations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(r => r.Provider)
                    .WithMany(u => u.ReceivedReservations)
                    .HasForeignKey(r => r.ProviderId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(r => r.Service)
                    .WithMany()
                    .HasForeignKey(r => r.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UnavailableDay
            modelBuilder.Entity<UnavailableDay>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.AdminId).IsRequired();
                e.Property(x => x.Date).HasColumnType("date");
            });
        }
    }
}