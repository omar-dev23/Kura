using System.ComponentModel.DataAnnotations.Schema;

namespace Kura.API.Models
{
    public class DoctorService
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
    }
}