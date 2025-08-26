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
            try
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
                if (user == null)
                    throw new GraphQLException("Pogrešan email ili lozinka.");

                var token = jwt.GenerateToken(user.Id, user.Email, user.Role);
                return new AuthPayload(token, user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LOGIN ERROR: {ex.Message}");
                throw new GraphQLException("Došlo je do greške prilikom prijave.");
            }
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
            try
            {
                var exists = await db.Users.AnyAsync(u => u.Email == email);
                if (exists) throw new GraphQLException("Korisnik s danim emailom već postoji.");

                if (role == "Admin" &&
                    (string.IsNullOrWhiteSpace(businessName) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(workHours)))
                {
                    throw new GraphQLException("Admin mora imati naziv obrta, adresu i radno vrijeme.");
                }

                string? dayRange = null;
                string? hourRange = null;
                List<string> workDays = new();

                if (!string.IsNullOrWhiteSpace(workHours))
                {
                    var partsParsed = workHours.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (partsParsed.Length >= 2)
                    {
                        dayRange = partsParsed[0].Trim();
                        hourRange = partsParsed[1].Trim();
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
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ADD_USER ERROR: {ex.Message}");
                throw new GraphQLException("Došlo je do greške prilikom registracije.");
            }
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

        // ostale metode (createService, createReservation, deleteReservation, itd.) ostaju nepromijenjene
        // jer trenutno nemamo indikaciju da one uzrokuju problem
        // Ako želiš, mogu i njih dodatno osigurati try-catch blokovima.
    }
}