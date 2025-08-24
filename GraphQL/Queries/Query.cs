using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Data;
using TerminoApp_NewBackend.Models;
using HotChocolate.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace TerminoApp_NewBackend.GraphQL.Queries
{
    public class Query
    {
        public string Hello() => "world";

        public Task<User?> GetUserByIdAsync(string id, [Service] AppDbContext db) =>
            db.Users.FirstOrDefaultAsync(u => u.Id == id);

        [Authorize]
        public async Task<User> Me([Service] AppDbContext db, ClaimsPrincipal claims)
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? claims.FindFirstValue("id");
            if (string.IsNullOrEmpty(userId))
                throw new GraphQLException("Nema user id u tokenu.");

            var user = await db.Users.FindAsync(userId);
            if (user is null) throw new GraphQLException("Korisnik nije pronađen.");
            return user;
        }

        [Authorize]
        [GraphQLName("myReservations")]
        public async Task<IEnumerable<Reservation>> GetMyReservations(
            ClaimsPrincipal claims,
            [Service] AppDbContext db)
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException("Nije moguće dohvatiti ID korisnika iz tokena.");
            }

            return await db.Reservations
                .Include(r => r.User)
                .Include(r => r.Service)
                .Include(r => r.Provider)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartsAt)
                .ToListAsync();
        }

        [Authorize(Roles = new[] { "Admin" })]
        [GraphQLName("myServices")]
        public async Task<List<Service>> GetMyServicesAsync(
            ClaimsPrincipal claims,
            [Service] AppDbContext db)
        {
            string? userId =
                claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                claims.FindFirst("sub")?.Value ??
                claims.FindFirst("uid")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("Nije moguće dohvatiti ID korisnika.")
                        .Build()
                );
            }

            return await db.Services
                .Where(s => s.ProviderId == userId)
                .ToListAsync();
        }

        public IQueryable<User> GetProviders([Service] AppDbContext db)
        {
            return db.Users.Where(u => u.Role == "Admin");
        }

        public async Task<IEnumerable<Service>> GetServices(
            [Service] AppDbContext db,
            string providerId)
        {
            return await db.Services
                .Where(s => s.ProviderId == providerId)
                .ToListAsync();
        }

        public async Task<Service?> GetServiceById(
            [Service] AppDbContext db,
            string id)
        {
            return await db.Services.FirstOrDefaultAsync(s => s.Id == id);
        }

        [Authorize]
        [GraphQLName("reservationsByProviderAndDate")]
        public async Task<IEnumerable<Reservation>> GetReservationsByProviderAndDateAsync(
            string providerId,
            [GraphQLType(typeof(DateTimeType))] DateTime date,
            [Service] AppDbContext db,
            [Service] ILogger<Query> logger) // ✅ injektirani logger
        {
            logger.LogInformation("🟡 POZIV: reservationsByProviderAndDate(providerId: {ProviderId}, date: {Date})", providerId, date);

            var start = date.Date;
            var end = start.AddDays(1);

            var result = await db.Reservations
                .Where(r => r.ProviderId == providerId &&
                            r.StartsAt >= start &&
                            r.StartsAt < end)
                .ToListAsync();

            logger.LogInformation("✅ REZULTAT: {count} rezervacija pronađeno za {providerId} na datum {date}", result.Count, providerId, date);

            foreach (var r in result)
            {
                logger.LogDebug("📌 Rezervacija: {start} - {duration}min", r.StartsAt, r.DurationMinutes);
            }

            return result;
        }
    }
}