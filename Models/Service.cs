using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerminoApp_NewBackend.Models
{
    public class Service
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; } = 30;

        public string ProviderId { get; set; } = string.Empty;
        public User Provider { get; set; } = default!;
    }
}