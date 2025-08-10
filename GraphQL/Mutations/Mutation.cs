using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Data;
using TerminoApp_NewBackend.Models;          // ✅ dodano za UnavailableDay
using TerminoApp_NewBackend.Services;
using TerminoApp_NewBackend.GraphQL.Inputs; // ✅ za UnavailableDayInput

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
    }
}