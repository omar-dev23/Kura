namespace Kura.API.Models
{
    public class DoctorWorkplace
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        // If linked to an existing organization
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        // If added manually
        public string? ManualName { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}