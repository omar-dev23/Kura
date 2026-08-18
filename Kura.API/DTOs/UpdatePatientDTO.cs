namespace Kura.API.DTOs
{
    public class UpdatePatientDTO
    {
        // Personal info
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }

        // Medical info
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicDiseases { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string? Gender { get; set; }
        public string? EmergencyContact { get; set; }
        public string? NationalId { get; set; }
        public string? VaccinationHistory { get; set; }
        public string? FamilyMedicalHistory { get; set; }
        public string? CurrentMedications { get; set; }
        public string? PastSurgeries { get; set; }
    }
}