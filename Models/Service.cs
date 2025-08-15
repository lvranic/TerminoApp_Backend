using System.ComponentModel.DataAnnotations;

namespace TerminoApp_NewBackend.Models
{
    public class Service
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int DurationMinutes { get; set; }

        public string ProviderId { get; set; } = default!;
        public User Provider { get; set; } = default!;
    }
}