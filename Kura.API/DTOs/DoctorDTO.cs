using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    // Doctor's own profile view
    public class DoctorProfileDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? ProfilePhoto { get; set; }

        // Professional
        public string Specialization { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Hospital { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string? AboutMe { get; set; }
        public double Rating { get; set; }
        public int RatingCount { get; set; }
        public bool IsVerified { get; set; }

        public List<CertificateDTO> Certificates { get; set; } = new();
        public List<ServiceDTO> Services { get; set; } = new();
    }

    // Public doctor profile (seen by patients)
    public class DoctorPublicProfileDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string Hospital { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string? AboutMe { get; set; }
        public double Rating { get; set; }
        public int RatingCount { get; set; }
        public bool IsVerified { get; set; }
        public List<CertificateDTO> Certificates { get; set; } = new();
        public List<ServiceDTO> Services { get; set; } = new();
    }

    // Update doctor profile
    public class UpdateDoctorDTO
    {
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "First name must be between 2 and 50 characters!")]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Last name must be between 2 and 50 characters!")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number!")]
        public string? PhoneNumber { get; set; }

        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Hospital { get; set; }
        public string? Address { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? AboutMe { get; set; }
    }

    // Certificate DTO
    public class CertificateDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public int Year { get; set; }
    }

    public class AddCertificateDTO
    {
        [Required(ErrorMessage = "Title is required!")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Institution is required!")]
        public string Institution { get; set; } = string.Empty;

        [Range(1950, 2100, ErrorMessage = "Please enter a valid year!")]
        public int Year { get; set; }
    }

    // Service DTO
    public class ServiceDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
    }

    public class AddServiceDTO
    {
        [Required(ErrorMessage = "Service name is required!")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Service name must be between 2 and 100 characters!")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, 100000, ErrorMessage = "Price must be a positive number!")]
        public decimal? Price { get; set; }
    }

    // Doctor search result
    public class DoctorSearchDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string Hospital { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public double Rating { get; set; }
        public bool IsVerified { get; set; }
    }

    // Assigned patient (doctor sees)
    public class AssignedPatientDTO
    {
        public int ConnectionId { get; set; }
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? Allergies { get; set; }
        public string? ChronicDiseases { get; set; }
        public DateTime ConnectedSince { get; set; }
    }

    // Full patient profile (doctor sees)
    public class PatientFullProfileDTO
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? Allergies { get; set; }
        public string? ChronicDiseases { get; set; }
        public string? EmergencyContact { get; set; }
        public string? NationalId { get; set; }
        public decimal Weight { get; set; }
        public decimal Height { get; set; }
        public string? VaccinationHistory { get; set; }
        public string? FamilyMedicalHistory { get; set; }
        public string? CurrentMedications { get; set; }
        public string? PastSurgeries { get; set; }
        public string? ProfilePhoto { get; set; }
        public List<PatientDocumentDTO> Documents { get; set; } = new();
    }

    // Patient document (doctor sees)
    public class PatientDocumentDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    // Rate a doctor
    public class RateDoctorDTO
    {
        [Required(ErrorMessage = "Rating is required!")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5!")]
        public int Rating { get; set; }
    }
}