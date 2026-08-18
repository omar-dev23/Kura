using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class CreateAppointmentDTO
    {
        [Required(ErrorMessage = "DoctorId is required!")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Date is required!")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Time is required!")]
        public string AppointmentTime { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    public class AppointmentResponseDTO
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? DoctorPhoto { get; set; }
        public string DoctorSpecialization { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DoctorHomeStatsDTO
    {
        public int TotalPatients { get; set; }
        public int Done { get; set; }
        public int Remaining { get; set; }
        public List<AppointmentResponseDTO> UpcomingAppointments { get; set; } = new();
    }

    public class UpdateAppointmentStatusDTO
    {
        [Required(ErrorMessage = "Status is required!")]
        [RegularExpression("^(Done|Cancelled)$",
            ErrorMessage = "Status must be 'Done' or 'Cancelled'!")]
        public string Status { get; set; } = string.Empty;
    }
}