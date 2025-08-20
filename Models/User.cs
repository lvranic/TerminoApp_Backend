namespace TerminoApp_NewBackend.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Ovi podaci su potrebni samo za Admina – zato su nullable
        public string? BusinessName { get; set; }
        public string? Address { get; set; }
        public string? WorkHours { get; set; }
    }
}