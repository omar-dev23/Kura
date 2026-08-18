using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class AddMedicineDTO
    {
        [Required(ErrorMessage = "Medicine name is required!")]
        public string MedicineName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dosage is required!")]
        public string Dosage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Times per day is required!")]
        public string TimesPerDay { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duration is required!")]
        public string Duration { get; set; } = string.Empty;
    }

    public class CreatePrescriptionDTO
    {
        [Required(ErrorMessage = "PatientId is required!")]
        public int PatientId { get; set; }

        public string? Notes { get; set; }

        [Required(ErrorMessage = "At least one medicine is required!")]
        [MinLength(1, ErrorMessage = "At least one medicine is required!")]
        public List<AddMedicineDTO> Medicines { get; set; } = new();
    }

    public class MedicineResponseDTO
    {
        public int Id { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string TimesPerDay { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }

    public class PrescriptionResponseDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MedicineResponseDTO> Medicines { get; set; } = new();
    }
}