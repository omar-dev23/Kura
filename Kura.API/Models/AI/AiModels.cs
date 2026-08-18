using System.Text.Json.Serialization;

namespace Kura.API.Models.AI
{
    public class KuraUploadResult
    {
        [JsonPropertyName("patient_id")]
        public string PatientId { get; set; } = string.Empty;

        [JsonPropertyName("total_files")]
        public int TotalFiles { get; set; }

        [JsonPropertyName("success_count")]
        public int SuccessCount { get; set; }

        [JsonPropertyName("error_count")]
        public int ErrorCount { get; set; }

        [JsonPropertyName("xray_count")]
        public int XrayCount { get; set; }

        [JsonPropertyName("report_count")]
        public int ReportCount { get; set; }

        public string Message { get; set; } = string.Empty;

        public List<KuraFileResult> Results { get; set; } = new();
    }

    public class KuraFileResult
    {
        public string Filename { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("document_id")]
        public string? DocumentId { get; set; }

        [JsonPropertyName("doc_type")]
        public string? DocType { get; set; }

        [JsonPropertyName("analysis_type")]
        public string? AnalysisType { get; set; }

        public string? Message { get; set; }
    }

    public class KuraHealthResult
    {
        public string Status { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string Ollama { get; set; } = string.Empty;
    }
}