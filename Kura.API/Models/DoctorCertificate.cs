namespace Kura.API.Models
{
    public class DoctorCertificate
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}