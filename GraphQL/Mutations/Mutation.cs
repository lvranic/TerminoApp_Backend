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
using TerminoApp_NewBackend.Services;
using TerminoApp_NewBackend.GraphQL.Inputs;
using TerminoApp_NewBackend.GraphQL.Payloads;

namespace TerminoApp_NewBackend.GraphQL.Mutations
{
    public class Mutation
    {
        public record AuthPayload(string Token, User User);

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
            if (exists)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("Korisnik s danim emailom već postoji.")
                        .Build()
                );
            }

            if (role == "Admin")
            {
                if (string.IsNullOrWhiteSpace(businessName) ||
                    string.IsNullOrWhiteSpace(address) ||
                    string.IsNullOrWhiteSpace(workHours))
                {
                    throw new GraphQLException(
                        ErrorBuilder.New()
                            .SetMessage("Admin mora imati naziv obrta, adresu i radno vrijeme.")
                            .Build()
                    );
                }
            }

            // ➤ Parsiraj radno vrijeme
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

            var normalized = input.Replace("-", "–"); // pretvori minus u en dash
            var tokens = normalized.Split('–');
            if (tokens.Length != 2) return allDays;

            var from = allDays.IndexOf(tokens[0].Trim());
            var to = allDays.IndexOf(tokens[1].Trim());

            if (from == -1 || to == -1) return allDays;

            if (from <= to)
                return allDays.GetRange(from, to - from + 1);

            // primjer: Pet–Uto
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
            string? userId =
                claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                claims.FindFirst("sub")?.Value ??
                claims.FindFirst("uid")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("Nije moguće odrediti korisnika iz tokena.")
                        .Build()
                );
            }

            bool providerOk = await db.Users.AnyAsync(u => u.Id == providerId);
            if (!providerOk)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("Neispravan providerId.")
                        .Build()
                );
            }

            var duration = durationMinutes ?? 30;

            var entity = new Reservation
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = userId,
                ProviderId = providerId,
                ServiceId = serviceId,
                StartsAt = DateTime.SpecifyKind(startsAtUtc, DateTimeKind.Utc),
                DurationMinutes = duration,
                Status = "Pending"
            };

            db.Reservations.Add(entity);
            await db.SaveChangesAsync();

            return new ReservationPayload(entity.Id, true, "OK");
        }
    }
}