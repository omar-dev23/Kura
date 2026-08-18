using Kura.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "First name is required!")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters!")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required!")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters!")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required!")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters!")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number!")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password!")]
        [Compare("Password", ErrorMessage = "Passwords do not match!")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required!")]
        [RegularExpression("^(Patient|Doctor|Organization)$",
    ErrorMessage = "Role must be 'Patient', 'Doctor', or 'Organization'!")]
        public string Role { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number!")]
        public string? PhoneNumber { get; set; }

        // Shared
        public string? NationalId { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }

        // Patient only
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicDiseases { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Address { get; set; }

        // Doctor only
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Hospital { get; set; }

        // Organization only
        public OrganizationType? OrganizationType { get; set; }
        public string? OrganizationName { get; set; }
        public string? WorkingHours { get; set; }
    }
}