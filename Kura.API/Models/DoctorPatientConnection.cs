namespace Kura.API.Models
{
    public enum ConnectionStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public class DoctorPatientConnection
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}