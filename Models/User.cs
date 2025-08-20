namespace TerminoApp_NewBackend.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }

        // ✅ Dodano
        public string BusinessName { get; set; }
        public string Address { get; set; }
        public string WorkHours { get; set; }
    }
}