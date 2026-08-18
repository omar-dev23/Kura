namespace Kura.API.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Refused,
        Upcoming,
        Done,
        Cancelled
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Upcoming;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}