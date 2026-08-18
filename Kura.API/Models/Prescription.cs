namespace Kura.API.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        // Navigation
        public ICollection<PrescriptionMedicine> Medicines { get; set; } = new List<PrescriptionMedicine>();
    }
}