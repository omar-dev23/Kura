namespace Kura.API.Models
{
    public enum OrgConnectionStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public class PatientOrganizationConnection
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public OrgConnectionStatus Status { get; set; } = OrgConnectionStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}