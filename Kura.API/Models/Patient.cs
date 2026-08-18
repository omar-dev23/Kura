using System.ComponentModel.DataAnnotations.Schema;

namespace Kura.API.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string NationalId { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string ChronicDiseases { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Weight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Height { get; set; }

        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ProfilePhoto { get; set; }

        // Medical History
        public string? VaccinationHistory { get; set; }
        public string? FamilyMedicalHistory { get; set; }
        public string? CurrentMedications { get; set; }
        public string? PastSurgeries { get; set; }

        // Navigation
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<DoctorPatientConnection> Connections { get; set; } = new List<DoctorPatientConnection>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<PatientOrganizationConnection> OrganizationConnections { get; set; } = new List<PatientOrganizationConnection>();
    }
}