using Kura.API.Data;
using Kura.API.DTOs;
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
    public class DoctorController : ControllerBase
    {
        private readonly KuraDbContext _context;

        public DoctorController(KuraDbContext context)
        {
            _context = context;
        }

        // GET /api/doctor/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Certificates)
                .Include(d => d.Services)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            return Ok(new DoctorProfileDTO
            {
                Id = doctor.Id,
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                Email = doctor.User.Email,
                PhoneNumber = doctor.User.PhoneNumber,
                Gender = doctor.Gender,
                NationalId = doctor.NationalId,
                Age = doctor.Age,
                ProfilePhoto = doctor.ProfilePhoto,
                Specialization = doctor.Specialization,
                LicenseNumber = doctor.LicenseNumber,
                Hospital = doctor.Hospital,
                Address = doctor.Address,
                YearsOfExperience = doctor.YearsOfExperience,
                AboutMe = doctor.AboutMe,
                Rating = doctor.Rating,
                RatingCount = doctor.RatingCount,
                IsVerified = doctor.IsVerified,
                Certificates = doctor.Certificates.Select(c => new CertificateDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    Institution = c.Institution,
                    Year = c.Year
                }).ToList(),
                Services = doctor.Services.Select(s => new ServiceDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price
                }).ToList()
            });
        }

        // PUT /api/doctor/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateDoctorDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                doctor.User.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                doctor.User.LastName = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                doctor.User.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Gender))
                doctor.Gender = dto.Gender;

            if (dto.Age.HasValue)
                doctor.Age = dto.Age.Value;

            if (!string.IsNullOrWhiteSpace(dto.Specialization))
                doctor.Specialization = dto.Specialization;

            if (!string.IsNullOrWhiteSpace(dto.LicenseNumber))
                doctor.LicenseNumber = dto.LicenseNumber;

            if (!string.IsNullOrWhiteSpace(dto.Hospital))
                doctor.Hospital = dto.Hospital;

            if (!string.IsNullOrWhiteSpace(dto.Address))
                doctor.Address = dto.Address;

            if (dto.YearsOfExperience.HasValue)
                doctor.YearsOfExperience = dto.YearsOfExperience.Value;

            if (!string.IsNullOrWhiteSpace(dto.AboutMe))
                doctor.AboutMe = dto.AboutMe;

            await _context.SaveChangesAsync();
            return Ok("Profile updated successfully!");
        }

        // POST /api/doctor/certificates
        [HttpPost("certificates")]
        public async Task<IActionResult> AddCertificate(AddCertificateDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var certificate = new DoctorCertificate
            {
                DoctorId = doctor.Id,
                Title = dto.Title,
                Institution = dto.Institution,
                Year = dto.Year
            };

            _context.DoctorCertificates.Add(certificate);
            await _context.SaveChangesAsync();

            return StatusCode(201, "Certificate added successfully!");
        }

        // DELETE /api/doctor/certificates/{id}
        [HttpDelete("certificates/{id}")]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var certificate = await _context.DoctorCertificates
                .FirstOrDefaultAsync(c => c.Id == id && c.DoctorId == doctor.Id);

            if (certificate == null)
                return NotFound("Certificate not found!");

            _context.DoctorCertificates.Remove(certificate);
            await _context.SaveChangesAsync();

            return Ok("Certificate deleted successfully!");
        }

        // POST /api/doctor/services
        [HttpPost("services")]
        public async Task<IActionResult> AddService(AddServiceDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var service = new DoctorService
            {
                DoctorId = doctor.Id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            };

            _context.DoctorServices.Add(service);
            await _context.SaveChangesAsync();

            return StatusCode(201, "Service added successfully!");
        }

        // DELETE /api/doctor/services/{id}
        [HttpDelete("services/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var service = await _context.DoctorServices
                .FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctor.Id);

            if (service == null)
                return NotFound("Service not found!");

            _context.DoctorServices.Remove(service);
            await _context.SaveChangesAsync();

            return Ok("Service deleted successfully!");
        }

        // GET /api/doctor/public/{doctorId}
        // Patient views a doctor's public profile
        [HttpGet("public/{doctorId}")]
        public async Task<IActionResult> GetPublicProfile(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Certificates)
                .Include(d => d.Services)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return NotFound("Doctor not found!");

            return Ok(new DoctorPublicProfileDTO
            {
                Id = doctor.Id,
                FullName = doctor.User.FirstName + " " + doctor.User.LastName,
                ProfilePhoto = doctor.ProfilePhoto,
                Specialization = doctor.Specialization,
                Hospital = doctor.Hospital,
                Address = doctor.Address,
                YearsOfExperience = doctor.YearsOfExperience,
                AboutMe = doctor.AboutMe,
                Rating = doctor.Rating,
                RatingCount = doctor.RatingCount,
                IsVerified = doctor.IsVerified,
                Certificates = doctor.Certificates.Select(c => new CertificateDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    Institution = c.Institution,
                    Year = c.Year
                }).ToList(),
                Services = doctor.Services.Select(s => new ServiceDTO
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList()
            });
        }

        // POST /api/doctor/rate/{doctorId}
        // Patient rates a doctor
        [HttpPost("rate/{doctorId}")]
        public async Task<IActionResult> RateDoctor(int doctorId, RateDoctorDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Patient")
                return StatusCode(403, "Only patients can rate doctors!");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return NotFound("Doctor not found!");

            // Calculate new average rating
            var totalRating = doctor.Rating * doctor.RatingCount + dto.Rating;
            doctor.RatingCount++;
            doctor.Rating = Math.Round(totalRating / doctor.RatingCount, 1);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Rating submitted!", newRating = doctor.Rating });
        }

        // GET /api/doctor/my-patients
        [HttpGet("my-patients")]
        public async Task<IActionResult> GetMyPatients()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var patients = await _context.DoctorPatientConnections
                .Include(c => c.Patient)
                    .ThenInclude(p => p.User)
                .Where(c => c.DoctorId == doctor.Id &&
                            c.Status == ConnectionStatus.Accepted)
                .Select(c => new AssignedPatientDTO
                {
                    ConnectionId = c.Id,
                    PatientId = c.Patient.Id,
                    FullName = c.Patient.User.FirstName + " " + c.Patient.User.LastName,
                    Email = c.Patient.User.Email,
                    PhoneNumber = c.Patient.User.PhoneNumber,
                    BloodType = c.Patient.BloodType,
                    Age = c.Patient.Age,
                    Gender = c.Patient.Gender,
                    Allergies = c.Patient.Allergies,
                    ChronicDiseases = c.Patient.ChronicDiseases,
                    ConnectedSince = c.RespondedAt ?? c.RequestedAt
                })
                .ToListAsync();

            return Ok(patients);
        }

        // GET /api/doctor/all
        // Returns all doctors with basic info
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Select(d => new
                {
                    d.Id,
                    FullName = d.User.FirstName + " " + d.User.LastName,
                    d.ProfilePhoto,
                    d.Specialization,
                    d.Hospital,
                    d.Rating,
                    d.YearsOfExperience,
                    d.IsVerified
                })
                .ToListAsync();

            return Ok(doctors);
        }

        // GET /api/doctor/my-patients/{patientId}
        [HttpGet("my-patients/{patientId}")]
        public async Task<IActionResult> GetPatientDetails(int patientId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var connection = await _context.DoctorPatientConnections
                .FirstOrDefaultAsync(c =>
                    c.DoctorId == doctor.Id &&
                    c.PatientId == patientId &&
                    c.Status == ConnectionStatus.Accepted);

            if (connection == null)
                return StatusCode(403, "This patient is not assigned to you!");

            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Documents)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                return NotFound("Patient not found!");

            return Ok(new PatientFullProfileDTO
            {
                PatientId = patient.Id,
                FullName = patient.User.FirstName + " " + patient.User.LastName,
                Email = patient.User.Email,
                PhoneNumber = patient.User.PhoneNumber,
                Address = patient.Address,
                BloodType = patient.BloodType,
                Age = patient.Age,
                Gender = patient.Gender,
                Allergies = patient.Allergies,
                ChronicDiseases = patient.ChronicDiseases,
                EmergencyContact = patient.EmergencyContact,
                NationalId = patient.NationalId,
                Weight = patient.Weight,
                Height = patient.Height,
                VaccinationHistory = patient.VaccinationHistory,
                FamilyMedicalHistory = patient.FamilyMedicalHistory,
                CurrentMedications = patient.CurrentMedications,
                PastSurgeries = patient.PastSurgeries,
                ProfilePhoto = patient.ProfilePhoto,
                Documents = patient.Documents.Select(d => new PatientDocumentDTO
                {
                    Id = d.Id,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    FileType = d.FileType,
                    HospitalName = d.HospitalName,
                    Notes = d.Notes,
                    DocumentDate = d.DocumentDate,
                    UploadedAt = d.UploadedAt
                }).ToList()
            });
        }
    }
}