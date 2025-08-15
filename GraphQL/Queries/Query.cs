// TerminoApp_NewBackend/GraphQL/Queries/Query.cs

using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Data;
using TerminoApp_NewBackend.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace TerminoApp_NewBackend.GraphQL.Queries
{
    public class Query
    {
        public string Hello() => "world";

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

        // ✅ NOVO: Dohvat svih admin korisnika (salona)
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
    }
}