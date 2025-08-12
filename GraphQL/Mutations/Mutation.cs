using System;
using System.Threading.Tasks;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Data;
using TerminoApp_NewBackend.Models;
using TerminoApp_NewBackend.Services;
using TerminoApp_NewBackend.GraphQL.Inputs;

namespace TerminoApp_NewBackend.GraphQL.Mutations
{
    public class Mutation
    {
        public record AuthPayload(string Token, User User);

        [GraphQLName("addUser")]
        public async Task<AuthPayload> AddUserAsync(
            string name,
            string email,
            string phone,
            string role,
            string password,
            [Service] AppDbContext db,
            [Service] JwtService jwt)
        {
            var exists = await db.Users.AnyAsync(u => u.Email == email);
            if (exists)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("Korisnik s danim emailom već postoji.")
                        .Build()
                );
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name,
                Email = email,
                Phone = phone,
                Role = role,
                // ⚠️ u produkciji hashirati lozinku
                Password = password
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = jwt.GenerateToken(user.Id, user.Email, user.Role);
            return new AuthPayload(token, user);
        }

        [GraphQLName("login")]
        public async Task<AuthPayload> LoginAsync(
            string email,
            string password,
            [Service] AppDbContext db,
            [Service] JwtService jwt)
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("Pogrešan email ili lozinka.")
                        .Build()
                );
            }

            var token = jwt.GenerateToken(user.Id, user.Email, user.Role);
            return new AuthPayload(token, user);
        }

        [GraphQLName("addUnavailableDay")]
        public async Task<UnavailableDay> AddUnavailableDay(
            [GraphQLNonNullType] UnavailableDayInput input,
            [Service] IDbContextFactory<AppDbContext> dbContextFactory)
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();

            var entity = new UnavailableDay
            {
                Date = input.Date.Date,
                AdminId = input.AdminId
            };

            context.UnavailableDays.Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        // =========================
        //   REZERVACIJE (NOVO)
        // =========================

        /// <summary>
        /// Kreira rezervaciju za trenutno prijavljenog korisnika.
        /// Vrijeme se prima kao UTC (startsAtUtc). Trajanje je opcionalno (default 30 min).
        /// </summary>
        // using System.Security.Claims;
        [Authorize]
        [GraphQLName("createReservation")]
        public async Task<Reservation> CreateReservationAsync(
            string providerId,
            string serviceId,
            DateTime startsAtUtc,
            int? durationMinutes,
            [Service] IDbContextFactory<AppDbContext> dbFactory,
            ClaimsPrincipal claims)
        {
            await using var db = await dbFactory.CreateDbContextAsync();

            var userId = claims.FindFirst("sub")?.Value
                      ?? claims.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? throw new GraphQLException(ErrorBuilder.New()
                           .SetMessage("Nije moguće odrediti korisnika iz tokena.")
                           .Build());

            var duration = durationMinutes ?? 30;

            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = userId,
                ProviderId = providerId,   // string
                ServiceId = serviceId,     // string
                StartsAt = DateTime.SpecifyKind(startsAtUtc, DateTimeKind.Utc),
                DurationMinutes = duration,
                Status = "Pending"
            };

            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();
            return reservation;
        }
    }
}