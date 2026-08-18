namespace Kura.API.Models
{
    public class Document
    {
        public int Id { get; set; }

       
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        

        public string DocumentType { get; set; } = string.Empty;
        

        public string FilePath { get; set; } = string.Empty;
      
        public string FileType { get; set; } = string.Empty;
        

        public string HospitalName { get; set; } = string.Empty;
       

        public string Notes { get; set; } = string.Empty;
        

        public string AiSummary { get; set; } = string.Empty;
        

        public string ExtractedText { get; set; } = string.Empty;
        

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        

        public DateTime DocumentDate { get; set; }
        

        public bool IsProcessed { get; set; } = false;
       
    }
}