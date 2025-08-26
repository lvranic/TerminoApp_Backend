using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Data;
using TerminoApp_NewBackend.Models;
using TerminoApp_NewBackend.GraphQL.Inputs;
using TerminoApp_NewBackend.GraphQL.Payloads;
using TerminoApp_NewBackend.Services;

namespace TerminoApp_NewBackend.GraphQL.Mutations
{
    public class Mutation
    {
        public record AuthPayload(string Token, User User);
        public record GenericPayload(bool Success, string Message);

        [GraphQLName("login")]
        public async Task<AuthPayload> LoginAsync(
            string email,
            string password,
            [Service] AppDbContext db,
            [Service] JwtService jwt)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user == null) throw new GraphQLException("Pogrešan email ili lozinka.");
            var token = jwt.GenerateToken(user.Id, user.Email, user.Role);
            return new AuthPayload(token, user);
        }

        [GraphQLName("addUser")]
        public async Task<AuthPayload> AddUserAsync(
            string name,
            string email,
            string phone,
            string role,
            string password,
            string? businessName,
            string? address,
            string? workHours,
            [Service] AppDbContext db,
            [Service] JwtService jwt)
        {
            var exists = await db.Users.AnyAsync(u => u.Email == email);
            if (exists) throw new GraphQLException("Korisnik s danim emailom već postoji.");

            var parts = workHours.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                dayRange = parts[0].Trim();       // npr: Pon-Pet
                hourRange = parts[1].Trim();      // npr: 09:00-17:00
                workDays = ParseDayRange(dayRange);
            }

            string? dayRange = null;
            string? hourRange = null;
            List<string> workDays = new();

            if (!string.IsNullOrWhiteSpace(workHours))
            {
                var parts = workHours.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    dayRange = parts[0].Trim();
                    hourRange = parts[1].Trim();
                    workDays = ParseDayRange(dayRange);
                }
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name,
                Email = email,
                Phone = phone,
                Role = role,
                Password = password,
                BusinessName = businessName,
                Address = address,
                WorkHours = workHours,
                WorkDays = workDays,
                WorkingHoursRange = hourRange
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = jwt.GenerateToken(user.Id, user.Email, user.Role);
            return new AuthPayload(token, user);
        }

        private List<string> ParseDayRange(string? input)
        {
            var allDays = new List<string> { "Pon", "Uto", "Sri", "Čet", "Pet", "Sub", "Ned" };
            if (string.IsNullOrWhiteSpace(input)) return allDays;

            var normalized = input.Replace("-", "–");
            var tokens = normalized.Split('–');
            if (tokens.Length != 2) return allDays;

            var from = allDays.IndexOf(tokens[0].Trim());
            var to = allDays.IndexOf(tokens[1].Trim());

            if (from == -1 || to == -1) return allDays;

            if (from <= to)
                return allDays.GetRange(from, to - from + 1);

            return allDays.Skip(from).Concat(allDays.Take(to + 1)).ToList();
        }

        [GraphQLName("createService")]
        public async Task<Service> CreateServiceAsync(
            string providerId,
            string name,
            int durationMinutes,
            [Service] AppDbContext db)
        {
            var service = new Service
            {
                Id = Guid.NewGuid().ToString("N"),
                ProviderId = providerId,
                Name = name,
                DurationMinutes = durationMinutes
            };

            db.Services.Add(service);
            await db.SaveChangesAsync();
            return service;
        }

        [Authorize]
        [GraphQLName("createReservation")]
        public async Task<ReservationPayload> CreateReservationAsync(
            string providerId,
            string serviceId,
            DateTime startsAtUtc,
            int? durationMinutes,
            ClaimsPrincipal claims,
            [Service] AppDbContext db)
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                throw new GraphQLException("Nije moguće odrediti korisnika iz tokena.");

            var provider = await db.Users.FindAsync(providerId);
            if (provider == null)
                throw new GraphQLException("Neispravan providerId.");

            var user = await db.Users.FindAsync(userId);
            if (user == null)
                throw new GraphQLException("Korisnik nije pronađen.");

            var service = await db.Services.FindAsync(serviceId);
            if (service == null)
                throw new GraphQLException("Usluga nije pronađena.");

            var duration = durationMinutes ?? 30;
            var endsAtUtc = startsAtUtc.AddMinutes(duration);

            var overlapping = await db.Reservations
                .Where(r => r.ProviderId == providerId &&
                            r.StartsAt < endsAtUtc &&
                            startsAtUtc < r.StartsAt.AddMinutes(r.DurationMinutes))
                .AnyAsync();

            if (overlapping)
                throw new GraphQLException("Odabrani termin se preklapa s postojećom rezervacijom.");

            var notification = new Notification
            {
                UserId = providerId,
                Message = $"Nova rezervacija za uslugu \"{service.Name}\" u {startsAtUtc.ToLocalTime():dd.MM.yyyy. HH:mm}",
            };
            db.Notifications.Add(notification);

            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = userId,
                ProviderId = providerId,
                ServiceId = serviceId,
                StartsAt = DateTime.SpecifyKind(startsAtUtc, DateTimeKind.Utc),
                DurationMinutes = duration,
                Status = "Pending"
            };

            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();

            return new ReservationPayload(reservation.Id, true, "OK");
        }

        [Authorize]
        [GraphQLName("deleteReservation")]
        public async Task<ReservationPayload> DeleteReservationAsync(
            string id,
            string? reason,
            ClaimsPrincipal claims,
            [Service] AppDbContext db)
        {
            var reservation = await db.Reservations
                .Include(r => r.User)
                .Include(r => r.Provider)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return new ReservationPayload(id, false, "Rezervacija nije pronađena.");

            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            var isUser = reservation.UserId == userId;
            var isProvider = reservation.ProviderId == userId;

            if (!isUser && !isProvider)
                throw new GraphQLException("Nedozvoljena akcija.");

            var serviceName = reservation.Service?.Name ?? "Nepoznata usluga";
            var dateStr = reservation.StartsAt.ToLocalTime().ToString("dd.MM.yyyy. HH:mm");

            if (isUser)
            {
                var msg = $"Korisnik je otkazao termin za uslugu \"{serviceName}\" u {dateStr}";
                if (!string.IsNullOrWhiteSpace(reason))
                    msg += $" – razlog: \"{reason}\"";

                db.Notifications.Add(new Notification
                {
                    UserId = reservation.ProviderId,
                    Message = msg
                });
            }

            if (isProvider)
            {
                var msg = $"Pružatelj usluge je otkazao vaš termin za \"{serviceName}\" u {dateStr}";
                if (!string.IsNullOrWhiteSpace(reason))
                    msg += $" – razlog: \"{reason}\"";

                db.Notifications.Add(new Notification
                {
                    UserId = reservation.UserId,
                    Message = msg
                });
            }

            // Obriši stare notifikacije za isti termin
            var relatedNotifications = await db.Notifications
                .Where(n => n.Message.Contains(dateStr))
                .ToListAsync();

            db.Notifications.RemoveRange(relatedNotifications);

            db.Reservations.Remove(reservation);
            await db.SaveChangesAsync();

            return new ReservationPayload(id, true, "Rezervacija je uspješno otkazana.");
        }

        [Authorize]
        [GraphQLName("updateService")]
        public async Task<Service> UpdateServiceAsync(
            string serviceId,
            int newDurationMinutes,
            [Service] AppDbContext db)
        {
            var service = await db.Services.FindAsync(serviceId);
            if (service == null)
                throw new GraphQLException("Usluga nije pronađena.");

            service.DurationMinutes = newDurationMinutes;
            await db.SaveChangesAsync();
            return service;
        }

        [Authorize]
        [GraphQLName("updateUser")]
        public async Task<User> UpdateUserAsync(
            string userId,
            string? address,
            string? phone,
            string? workHours,
            [Service] AppDbContext db)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null)
                throw new GraphQLException("Korisnik nije pronađen.");

            user.Address = address ?? user.Address;
            user.Phone = phone ?? user.Phone;
            user.WorkHours = workHours ?? user.WorkHours;

            if (!string.IsNullOrWhiteSpace(workHours))
            {
                var parts = workHours.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var dayRange = parts[0].Trim();
                    var hourRange = parts[1].Trim();
                    user.WorkingHoursRange = hourRange;
                    user.WorkDays = ParseDayRange(dayRange);
                }
            }

            await db.SaveChangesAsync();
            return user;
        }

        [Authorize]
        [GraphQLName("markAllNotificationsAsRead")]
        public async Task<bool> MarkAllNotificationsAsReadAsync(
            ClaimsPrincipal claims,
            [Service] AppDbContext db)
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return false;

            var unread = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await db.SaveChangesAsync();
            return true;
        }

        [Authorize]
        [GraphQLName("markNotificationAsRead")]
        public async Task<GenericPayload> MarkNotificationAsReadAsync(
            string id,
            ClaimsPrincipal claims,
            [Service] AppDbContext db)
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new GenericPayload(false, "Korisnik nije prijavljen.");

            var notification = await db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return new GenericPayload(false, "Notifikacija nije pronađena.");

            notification.IsRead = true;
            await db.SaveChangesAsync();

            return new GenericPayload(true, "Notifikacija označena kao pročitana.");
        }
    }
}