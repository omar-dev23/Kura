using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class SendConnectionRequestDTO
    {
        [Required(ErrorMessage = "DoctorId is required!")]
        public int DoctorId { get; set; }
    }

    public class ConnectionResponseDTO
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }

        public DoctorInfoDTO? Doctor { get; set; }

        public PatientInfoDTO? Patient { get; set; }
    }

    public class DoctorInfoDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Hospital { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string? ProfilePhoto { get; set; }
    }

    public class PatientInfoDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
    }

    public class SearchDoctorDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Hospital { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }
}