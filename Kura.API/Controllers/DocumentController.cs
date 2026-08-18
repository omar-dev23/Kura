using Kura.API.Data;
using Kura.API.Interfaces;
using Kura.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kura.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly KuraDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;

        public DocumentController(
            KuraDbContext context,
            IWebHostEnvironment environment,
            INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _notificationService = notificationService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            IFormFile file,
            [FromForm] string title,
            [FromForm] string documentType,
            [FromForm] string hospitalName,
            [FromForm] string notes,
            [FromForm] DateTime documentDate)
        {
            var userId = int.Parse(User.FindFirst
                (ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient not found!");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded!");

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("File too large! Max 10MB");

            var allowedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedTypes.Contains(fileExtension))
                return BadRequest("Only PDF, JPG, PNG allowed!");

            var uploadsFolder = Path.Combine(
                _environment.ContentRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                PatientId = patient.Id,
                Title = title,
                DocumentType = documentType,
                FilePath = filePath,
                FileType = fileExtension.Replace(".", "").ToUpper(),
                HospitalName = hospitalName,
                Notes = notes,
                DocumentDate = documentDate,
                UploadedAt = DateTime.UtcNow,
                IsProcessed = false
            };

            _context.Documents.Add(document);
            await _notificationService.SendAsync(
                patient.UserId,
                "Document Uploaded",
                $"Your document '{title}' was uploaded successfully!"
            );

            return StatusCode(201, new
            {
                Message = "Document uploaded successfully!",
                DocumentId = document.Id,
                FileName = uniqueFileName,
                FileType = document.FileType
            });
        }

        [HttpGet("myfiles")]
        public async Task<IActionResult> GetMyDocuments()
        {
            var userId = int.Parse(User.FindFirst
                (ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient not found!");

            var documents = await _context.Documents
                .Where(d => d.PatientId == patient.Id)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.DocumentType,
                    d.FileType,
                    d.HospitalName,
                    d.Notes,
                    d.DocumentDate,
                    d.UploadedAt,
                    d.IsProcessed,
                    d.AiSummary
                })
                .ToListAsync();

            return Ok(documents);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient not found!");

            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id && d.PatientId == patient.Id);

            if (document == null)
                return NotFound("Document not found!");

            if (System.IO.File.Exists(document.FilePath))
            {
                System.IO.File.Delete(document.FilePath);
            }

            _context.Documents.Remove(document);
            await _notificationService.SendAsync(
                patient.UserId,
                "Document Deleted",
                $"Your document '{document.Title}' was deleted."
            );

            return Ok("Document deleted successfully!");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ViewDocument(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            Document? document = null;

            if (role == "Patient")
            {
                // Patient can only view their own documents
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                    return NotFound("Patient not found!");

                document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == id && d.PatientId == patient.Id);
            }
            else if (role == "Doctor")
            {
                // Doctor can only view documents of assigned patients
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                    return NotFound("Doctor not found!");

                document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                    return NotFound("Document not found!");

                // Make sure patient is assigned to this doctor
                var connection = await _context.DoctorPatientConnections
                    .FirstOrDefaultAsync(c =>
                        c.DoctorId == doctor.Id &&
                        c.PatientId == document.PatientId &&
                        c.Status == ConnectionStatus.Accepted);

                if (connection == null)
                    return StatusCode(403,
                        "This document belongs to a patient not assigned to you!");
            }
            else
            {
                return StatusCode(403, "Access denied!");
            }

            if (document == null)
                return NotFound("Document not found!");

            if (!System.IO.File.Exists(document.FilePath))
                return NotFound("File not found on server!");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);

            var contentType = document.FileType.ToLower() switch
            {
                "pdf" => "application/pdf",
                "jpg" => "image/jpeg",
                "jpeg" => "image/jpeg",
                "png" => "image/png",
                _ => "application/octet-stream"
            };

            return File(fileBytes, contentType, Path.GetFileName(document.FilePath));
        }
    }
}