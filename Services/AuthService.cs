namespace TerminoApp_NewBackend.Services
{
    public class AuthService
    {
        // Ovo je samo primjer!
        public Task<string> LoginAsync(string email, string password)
        {
            // provjera korisnika, vraćanje tokena itd.
            return Task.FromResult("mock-token");
        }
    }
}