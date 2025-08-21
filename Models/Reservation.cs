// Models/Reservation.cs
using System;

namespace TerminoApp_NewBackend.Models
{
    public class Reservation
    {
        public string Id { get; set; } = default!;

        public string ProviderId { get; set; } = default!;
        public string ServiceId { get; set; } = default!;
        public string UserId { get; set; } = default!;

        public DateTime StartsAt { get; set; }

        public int DurationMinutes { get; set; }
        public string Status { get; set; } = "Pending";

        // Navigacijske veze
        public User User { get; set; } = default!;
        public User Provider { get; set; } = default!;
        public Service Service { get; set; } = default!;
    }
}