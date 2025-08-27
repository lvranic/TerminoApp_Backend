// GraphQL/Payloads/LoginPayload.cs
using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.GraphQL.Payloads
{
    public class LoginPayload
    {
        public string Token { get; set; } = default!;
        public User User { get; set; } = default!;
    }
}