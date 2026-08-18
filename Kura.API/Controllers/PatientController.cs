using Kura.API.Data;
using Kura.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kura.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly KuraDbContext _context;

        public PatientController(KuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst
                (ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient profile not found!");

            return Ok(new
            {
                patient.User.FirstName,
                patient.User.LastName,
                patient.User.Email,
                patient.User.PhoneNumber,
                patient.ProfilePhoto,
                patient.NationalId,
                patient.Age,
                patient.Gender,
                patient.Address,
                patient.BloodType,
                patient.Allergies,
                patient.ChronicDiseases,
                patient.Weight,
                patient.Height,
                patient.EmergencyContact,
                patient.VaccinationHistory,
                patient.FamilyMedicalHistory,
                patient.CurrentMedications,
                patient.PastSurgeries,
                Documents = patient.Documents?.Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.DocumentType,
                    d.FileType,
                    d.HospitalName,
                    d.Notes,
                    d.DocumentDate,
                    d.UploadedAt
                })
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdatePatientDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient profile not found!");

            if (dto.Weight.HasValue && dto.Weight <= 0)
                return BadRequest("Weight must be greater than 0!");

            if (dto.Height.HasValue && dto.Height <= 0)
                return BadRequest("Height must be greater than 0!");

            if (dto.Age.HasValue)
                patient.Age = dto.Age.Value;

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                patient.User.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                patient.User.LastName = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                patient.User.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.BloodType))
                patient.BloodType = dto.BloodType;

            if (!string.IsNullOrWhiteSpace(dto.Allergies))
                patient.Allergies = dto.Allergies;

            if (!string.IsNullOrWhiteSpace(dto.ChronicDiseases))
                patient.ChronicDiseases = dto.ChronicDiseases;

            if (dto.Weight.HasValue)
                patient.Weight = dto.Weight.Value;

            if (dto.Height.HasValue)
                patient.Height = dto.Height.Value;

            if (!string.IsNullOrWhiteSpace(dto.Gender))
                patient.Gender = dto.Gender;

            if (!string.IsNullOrWhiteSpace(dto.Address))
                patient.Address = dto.Address;

            if (!string.IsNullOrWhiteSpace(dto.EmergencyContact))
                patient.EmergencyContact = dto.EmergencyContact;

            if (!string.IsNullOrWhiteSpace(dto.NationalId))
                patient.NationalId = dto.NationalId;

            if (!string.IsNullOrWhiteSpace(dto.VaccinationHistory))
                patient.VaccinationHistory = dto.VaccinationHistory;

            if (!string.IsNullOrWhiteSpace(dto.FamilyMedicalHistory))
                patient.FamilyMedicalHistory = dto.FamilyMedicalHistory;

            if (!string.IsNullOrWhiteSpace(dto.CurrentMedications))
                patient.CurrentMedications = dto.CurrentMedications;

            if (!string.IsNullOrWhiteSpace(dto.PastSurgeries))
                patient.PastSurgeries = dto.PastSurgeries;

            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully!");
        }

        // POST /api/patient/upload-photo
        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto([FromBody] UploadPhotoDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Patient")
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                    return NotFound("Patient not found!");

                // Validate base64
                if (string.IsNullOrWhiteSpace(dto.Base64Image))
                    return BadRequest("Image is required!");

                patient.ProfilePhoto = dto.Base64Image;
                await _context.SaveChangesAsync();

                return Ok("Profile photo updated successfully!");
            }
            else if (role == "Doctor")
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                    return NotFound("Doctor not found!");

                if (string.IsNullOrWhiteSpace(dto.Base64Image))
                    return BadRequest("Image is required!");

                doctor.ProfilePhoto = dto.Base64Image;
                await _context.SaveChangesAsync();

                return Ok("Profile photo updated successfully!");
            }

            return StatusCode(403, "Access denied!");
        }
    }
}