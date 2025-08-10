using TerminoApp_NewBackend.Data;
using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.GraphQL.Mutations
{
    public class UserMutation
    {
        public async Task<User> AddUser(User input, [Service] AppDbContext context)
        {
            context.Users.Add(input);
            await context.SaveChangesAsync();
            return input;
        }
    }
}