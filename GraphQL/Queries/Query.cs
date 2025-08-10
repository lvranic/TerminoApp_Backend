// TerminoApp_NewBackend/GraphQL/Queries/Query.cs
using HotChocolate;
using HotChocolate.Types;
// (dodaj sljedeće using-e po potrebi)
using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Data;
using TerminoApp_NewBackend.Models;
using HotChocolate.Authorization; // vidi točku 2
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
        }
}