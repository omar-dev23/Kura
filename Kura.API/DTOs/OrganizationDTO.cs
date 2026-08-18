using System.ComponentModel.DataAnnotations;
using Kura.API.Models;

namespace Kura.API.DTOs
{
    public class OrganizationProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? WorkingHours { get; set; }
        public string? LicenseNumber { get; set; }
        public double Rating { get; set; }
        public int RatingCount { get; set; }
        public bool IsVerified { get; set; }
        public List<OrgServiceDTO> Services { get; set; } = new();
        public List<OrgDepartmentDTO> Departments { get; set; } = new();
        // Hospital only
        public List<OrgSpecialtyDTO> Specialties { get; set; } = new();

        // Clinic only
        public string? Specialty { get; set; }

        // Pharmacy only
        public List<OrgPharmacistDTO> Pharmacists { get; set; } = new();
        public int TotalDrugs { get; set; }

        // Lab only
        public List<OrgLabDoctorDTO> Doctors { get; set; } = new();
        public int TotalDevices { get; set; }
    }

    public class OrganizationListDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public double Rating { get; set; }
        public bool IsVerified { get; set; }
    }

    public class UpdateOrganizationDTO
    {
        public string? Name { get; set; }
        public string? Address { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number!")]
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
        public string? WorkingHours { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Specialty { get; set; }      // Clinic only
        public int? TotalDrugs { get; set; }        // Pharmacy only
        public int? TotalDevices { get; set; }      // Lab only
    }

    public class OrgServiceDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AddOrgServiceDTO
    {
        [Required(ErrorMessage = "Service name is required!")]
        public string Name { get; set; } = string.Empty;
    }

    public class OrgDepartmentDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AddOrgDepartmentDTO
    {
        [Required(ErrorMessage = "Department name is required!")]
        public string Name { get; set; } = string.Empty;
    }

    public class RateOrganizationDTO
    {
        [Required(ErrorMessage = "Rating is required!")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5!")]
        public int Rating { get; set; }
    }

    public class OrgSpecialtyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AddOrgSpecialtyDTO
    {
        [Required(ErrorMessage = "Specialty name is required!")]
        public string Name { get; set; } = string.Empty;
    }

    public class OrgPharmacistDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AddOrgPharmacistDTO
    {
        [Required(ErrorMessage = "Pharmacist name is required!")]
        public string Name { get; set; } = string.Empty;
    }

    public class OrgLabDoctorDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AddOrgLabDoctorDTO
    {
        [Required(ErrorMessage = "Doctor name is required!")]
        public string Name { get; set; } = string.Empty;
    }

    public class UploadOrgPhotoDTO
    {
        [Required(ErrorMessage = "Image is required!")]
        public string Base64Image { get; set; } = string.Empty;
    }
}