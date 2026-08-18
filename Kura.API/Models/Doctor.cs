namespace Kura.API.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Basic Info
        public string Specialization { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Hospital { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsVerified { get; set; } = false;

        // Professional Info
        public int YearsOfExperience { get; set; }
        public string? AboutMe { get; set; }
        public string? ProfilePhoto { get; set; }

        // Rating
        public double Rating { get; set; } = 0;
        public int RatingCount { get; set; } = 0;

        // Navigation
        public ICollection<DoctorCertificate> Certificates { get; set; } = new List<DoctorCertificate>();
        public ICollection<DoctorService> Services { get; set; } = new List<DoctorService>();
        public ICollection<DoctorPatientConnection> Connections { get; set; } = new List<DoctorPatientConnection>();
        public ICollection<DoctorWorkplace> Workplaces { get; set; } = new List<DoctorWorkplace>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}