// GraphQL/Mutations/UserMutation.cs

using System;
using System.Threading.Tasks;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Data;
using TerminoApp_NewBackend.Models;
using TerminoApp_NewBackend.Services;
using TerminoApp_NewBackend.GraphQL.Payloads;

namespace TerminoApp_NewBackend.GraphQL.Mutations
{
    public class UserMutation
    {
        public async Task<LoginPayload> Login(
            [Service] AppDbContext db,
            [Service] JwtService jwt,
            string email,
            string password)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.Password != password)
            {
                throw new GraphQLException("Neispravni podaci za prijavu.");
            }

            var token = jwt.Generate(user);

            return new LoginPayload
            {
                Token = token,
                User = user
            };
        }

        public async Task<Service> CreateService(
            [Service] AppDbContext db,
            string providerId,
            string name,
            int durationMinutes)
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
        public async Task<Reservation> CreateReservation(
            [Service] AppDbContext db,
            string providerId,
            string serviceId,
            DateTime startsAtUtc,
            int duration,
            ClaimsPrincipal claims)
        {
            string? userId =
                claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                claims.FindFirst("sub")?.Value ??
                claims.FindFirst("uid")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new GraphQLException("Nije moguće odrediti korisnika iz tokena.");
            }

            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString("N"),
                ProviderId = providerId,
                ServiceId = serviceId,
                UserId = userId,
                StartsAt = startsAtUtc,
                DurationMinutes = duration,
                Status = "Pending"
            };

            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();
            return reservation;
        }
    }
}