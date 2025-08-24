using System;
using System.ComponentModel.DataAnnotations;

namespace TerminoApp_NewBackend.Models
{
    public class UnavailableDay
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string AdminId { get; set; } = default!; // 🔧 riješen warning za CS8618
    }
}