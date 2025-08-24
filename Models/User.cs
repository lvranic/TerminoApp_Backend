using System.Collections.Generic;

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

        // Admin only
        public string? BusinessName { get; set; }
        public string? Address { get; set; }
        public string? WorkHours { get; set; } // npr. "Pon-Pet 9-17"
        public List<string>? WorkDays { get; set; }  // npr. ["Pon", "Uto", ...]
        public string? WorkingHoursRange { get; set; } // npr. "9-17"

        // Navigacijske veze
        public List<Service> Services { get; set; } = new();
        public List<Reservation> MyReservations { get; set; } = new(); // kao User
        public List<Reservation> ReceivedReservations { get; set; } = new(); // kao Provider
    }
}