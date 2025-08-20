// Models/Reservation.cs
using System;

namespace TerminoApp_NewBackend.Models
{
    public class Reservation
    {
        public string Id { get; set; }
        public string ProviderId { get; set; }
        public string ServiceId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }  // navigacijska veza
        public Service Service { get; set; }
        public DateTime StartsAt { get; set; } // ⬅️ OVO OBAVEZNO DODAJ

        public int DurationMinutes { get; set; }
        public string Status { get; set; }
    }
}