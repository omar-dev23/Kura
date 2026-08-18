using System.Net.Http.Headers;
using System.Net.Http.Json;
using Kura.API.Models.AI;

namespace Kura.API.Services
{
    public class KuraAIService
    {
        private readonly HttpClient _http;
        private readonly ILogger<KuraAIService> _logger;

        public KuraAIService(HttpClient http, ILogger<KuraAIService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // Get base URL for debugging
        public string GetBaseUrl()
        {
            return _http.BaseAddress?.ToString() ?? "No base URL set";
        }

        // Upload documents to AI service
        public async Task<KuraUploadResult?> UploadDocumentsAsync(
            string patientId,
            List<IFormFile> files)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(patientId), "patient_id");

                // Debug log
                Console.WriteLine($"[AI Service] Sending patient_id: {patientId}, files count: {files.Count}");
                foreach (var f in files)
                    Console.WriteLine($"[AI Service] File: {f.FileName}, Size: {f.Length}, ContentType: {f.ContentType}");

                foreach (var file in files)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType =
                        new MediaTypeHeaderValue(
                            string.IsNullOrEmpty(file.ContentType)
                                ? "application/octet-stream"
                                : file.ContentType);
                    form.Add(streamContent, "files", file.FileName);
                }

                var response = await _http.PostAsync("/documents/upload", form);

                Console.WriteLine($"[AI Service] Response status: {response.StatusCode}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AI Service] Response body: {responseBody}");

                response.EnsureSuccessStatusCode();
                return System.Text.Json.JsonSerializer.Deserialize<KuraUploadResult>(responseBody,
                new System.Text.Json.JsonSerializerOptions {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading documents to AI service");
                Console.WriteLine($"[AI Service] Exception: {ex.Message}");
                return null;
            }
        }

        // Get full summary as string
        public async Task<string?> GetSummaryAsync(string patientId, string lang = "ar")
        {
            try
            {
                var response = await _http.GetAsync(
                    $"/query/summary/{patientId}?lang={lang}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting summary from AI service");
                return null;
            }
        }

        // Stream summary chunk by chunk
        public async IAsyncEnumerable<string> StreamSummaryAsync(
            string patientId,
            string lang = "ar")
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/query/summary/{patientId}?lang={lang}");

            using var response = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var chunk = await reader.ReadLineAsync();
                if (chunk != null) yield return chunk + "\n";
            }
        }

        // Get full patterns as string
        public async Task<string?> GetPatternsAsync(
            string patientId,
            int? age = null,
            string? gender = null,
            string? chronicDiseases = null,
            int monthsAgo = 3)
        {
            try
            {
                var url = $"/patterns/{patientId}?months_ago={monthsAgo}";
                if (age.HasValue) url += $"&age={age}";
                if (!string.IsNullOrEmpty(gender))
                    url += $"&gender={Uri.EscapeDataString(gender)}";
                if (!string.IsNullOrEmpty(chronicDiseases))
                    url += $"&chronic_diseases={Uri.EscapeDataString(chronicDiseases)}";

                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patterns from AI service");
                return null;
            }
        }

        // Stream patterns chunk by chunk
        public async IAsyncEnumerable<string> StreamPatternsAsync(
            string patientId,
            int monthsAgo = 3,
            int? age = null,
            string? gender = null,
            string? chronicDiseases = null)
        {
            var url = $"/patterns/{patientId}?months_ago={monthsAgo}";
            if (age.HasValue) url += $"&age={age}";
            if (!string.IsNullOrEmpty(gender))
                url += $"&gender={Uri.EscapeDataString(gender)}";
            if (!string.IsNullOrEmpty(chronicDiseases))
                url += $"&chronic_diseases={Uri.EscapeDataString(chronicDiseases)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var chunk = await reader.ReadLineAsync();
                if (chunk != null) yield return chunk + "\n";
            }
        }

        // Health check
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _http.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}