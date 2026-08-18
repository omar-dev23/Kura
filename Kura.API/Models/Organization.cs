namespace Kura.API.Models
{
    public enum OrganizationType
    {
        Hospital,
        Clinic,
        Pharmacy,
        Laboratory
    }

    public class Organization
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public OrganizationType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? WorkingHours { get; set; }
        public string? LicenseNumber { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? Specialty { get; set; }

        // Pharmacy only
        public int TotalDrugs { get; set; } = 0;

        // Lab only
        public int TotalDevices { get; set; } = 0;

        // Rating
        public double Rating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;

        // Navigation
        public ICollection<OrganizationService> Services { get; set; } = new List<OrganizationService>();
        public ICollection<OrganizationDepartment> Departments { get; set; } = new List<OrganizationDepartment>();
        public ICollection<OrganizationSpecialty> Specialties { get; set; } = new List<OrganizationSpecialty>();
        public ICollection<OrganizationPharmacist> Pharmacists { get; set; } = new List<OrganizationPharmacist>();
        public ICollection<OrganizationLabDoctor> LabDoctors { get; set; } = new List<OrganizationLabDoctor>();
        public ICollection<PatientOrganizationConnection> PatientConnections { get; set; } = new List<PatientOrganizationConnection>();
    }
}