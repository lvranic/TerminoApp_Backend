// Models/Reservation.cs
using System;

namespace TerminoApp_NewBackend.Models
{
    public class Reservation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string UserId { get; set; } = default!;
        public string ProviderId { get; set; } = default!;
        public string ServiceId { get; set; } = default!;
        public DateTime StartsAt { get; set; }      // UTC
        public int DurationMinutes { get; set; }    // npr. 30
        public string Status { get; set; } = "Pending";
    }
}