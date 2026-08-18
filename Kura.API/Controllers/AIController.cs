using Kura.API.Data;
using Kura.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kura.API.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly KuraAIService _ai;
        private readonly KuraDbContext _context;

        public AIController(KuraAIService ai, KuraDbContext context)
        {
            _ai = ai;
            _context = context;
        }

        // POST /api/documents/upload/{patientId}
        [HttpPost("upload/{patientId}")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UploadDocuments(
    string patientId,
    [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "No files were sent!" });

            if (files.Count > 10)
                return BadRequest(new { message = "Maximum 10 files allowed!" });

            // Simply use the patientId from the URL directly
            // No need to look it up — just pass it to AI service
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest(new { message = "patientId is required!" });

            try
            {
                var result = await _ai.UploadDocumentsAsync(patientId, files);

                if (result == null)
                    return StatusCode(503, new { message = "AI Service is currently unavailable!" });

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(503, new { message = "AI Service is currently unavailable!" });
            }
        }

        // GET /api/documents/summary/{patientId}
        // Returns full summary as string
        [HttpGet("summary/{patientId}")]
        public async Task<IActionResult> GetSummary(
            string patientId,
            [FromQuery] string lang = "ar")
        {
            try
            {
                var summary = await _ai.GetSummaryAsync(patientId, lang);

                if (summary == null)
                    return StatusCode(503, new { message = "AI Service is currently unavailable!" });

                return Ok(new { summary });
            }
            catch (Exception)
            {
                return StatusCode(503, new { message = "AI Service is currently unavailable!" });
            }
        }

        // GET /api/documents/summary/{patientId}/stream
        // Streams summary chunk by chunk to Flutter
        [HttpGet("summary/{patientId}/stream")]
        public async Task StreamSummary(
            string patientId,
            [FromQuery] string lang = "ar")
        {
            Response.ContentType = "text/plain; charset=utf-8";
            Response.Headers.Append("X-Accel-Buffering", "no");

            try
            {
                await foreach (var chunk in _ai.StreamSummaryAsync(patientId, lang))
                {
                    await Response.WriteAsync(chunk);
                    await Response.Body.FlushAsync();
                }
            }
            catch (Exception)
            {
                await Response.WriteAsync(
                    "AI Service is currently unavailable. Please try again later.");
            }
        }

        // GET /api/documents/patterns/{patientId}
        // Returns full patterns as string
        [HttpGet("patterns/{patientId}")]
        public async Task<IActionResult> GetPatterns(
            string patientId,
            [FromQuery] int monthsAgo = 3)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            int? age = null;
            string? gender = null;
            string? chronicDiseases = null;

            // Get patient info for better pattern analysis
            if (role == "Patient")
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient != null)
                {
                    age = patient.Age;
                    gender = patient.Gender;
                    chronicDiseases = patient.ChronicDiseases;
                }
            }
            else if (role == "Doctor")
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.Id.ToString() == patientId);

                if (patient != null)
                {
                    age = patient.Age;
                    gender = patient.Gender;
                    chronicDiseases = patient.ChronicDiseases;
                }
            }

            try
            {
                var patterns = await _ai.GetPatternsAsync(
                    patientId, age, gender, chronicDiseases, monthsAgo);

                if (patterns == null)
                    return StatusCode(503, new { message = "AI Service is currently unavailable!" });

                return Ok(new { patterns });
            }
            catch (Exception)
            {
                return StatusCode(503, new { message = "AI Service is currently unavailable!" });
            }
        }

        // GET /api/documents/ai-health
        // Check if AI Service is reachable
        [HttpGet("ai-health")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckHealth()
        {
            var isHealthy = await _ai.IsHealthyAsync();

            if (isHealthy)
                return Ok(new { status = "AI Service is healthy ✅" });

            return StatusCode(503, new { status = "AI Service is unreachable ❌" });
        }
    }
}