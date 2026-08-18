using Kura.API.Data;
using Kura.API.DTOs;
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
    public class PrescriptionController : ControllerBase
    {
        private readonly KuraDbContext _context;
        private readonly INotificationService _notificationService;

        public PrescriptionController(
            KuraDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // POST /api/prescription
        // Doctor writes a prescription for a patient
        [HttpPost]
        public async Task<IActionResult> CreatePrescription(CreatePrescriptionDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can write prescriptions!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            // Make sure patient is assigned to this doctor
            var connection = await _context.DoctorPatientConnections
                .FirstOrDefaultAsync(c =>
                    c.DoctorId == doctor.Id &&
                    c.PatientId == dto.PatientId &&
                    c.Status == ConnectionStatus.Accepted);

            if (connection == null)
                return StatusCode(403, "This patient is not assigned to you!");

            // Get patient for notification
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == dto.PatientId);

            if (patient == null)
                return NotFound("Patient not found!");

            // Create prescription
            var prescription = new Prescription
            {
                DoctorId = doctor.Id,
                PatientId = dto.PatientId,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                Medicines = dto.Medicines.Select(m => new PrescriptionMedicine
                {
                    MedicineName = m.MedicineName,
                    Dosage = m.Dosage,
                    TimesPerDay = m.TimesPerDay,
                    Duration = m.Duration
                }).ToList()
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            // Notify the patient
            await _notificationService.SendAsync(
                patient.UserId,
                "New Prescription",
                $"Dr. {doctor.User.FirstName} {doctor.User.LastName} has written you a new prescription!");

            return StatusCode(201, new
            {
                Message = "Prescription created successfully!",
                PrescriptionId = prescription.Id
            });
        }

        // GET /api/prescription/my-prescriptions
        // Patient views their own prescriptions
        [HttpGet("my-prescriptions")]
        public async Task<IActionResult> GetMyPrescriptions()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Patient")
                return StatusCode(403, "Only patients can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient profile not found!");

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.User)
                .Include(p => p.Patient)
                    .ThenInclude(p => p.User)
                .Include(p => p.Medicines)
                .Where(p => p.PatientId == patient.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PrescriptionResponseDTO
                {
                    Id = p.Id,
                    PatientId = p.PatientId,
                    PatientName = p.Patient.User.FirstName + " " + p.Patient.User.LastName,
                    DoctorId = p.DoctorId,
                    DoctorName = "Dr. " + p.Doctor.User.FirstName + " " + p.Doctor.User.LastName,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt,
                    Medicines = p.Medicines.Select(m => new MedicineResponseDTO
                    {
                        Id = m.Id,
                        MedicineName = m.MedicineName,
                        Dosage = m.Dosage,
                        TimesPerDay = m.TimesPerDay,
                        Duration = m.Duration
                    }).ToList()
                })
                .ToListAsync();

            return Ok(prescriptions);
        }

        // GET /api/prescription/patient/{patientId}
        // Doctor views prescriptions they wrote for a specific patient
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientPrescriptions(int patientId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            // Make sure patient is assigned to this doctor
            var connection = await _context.DoctorPatientConnections
                .FirstOrDefaultAsync(c =>
                    c.DoctorId == doctor.Id &&
                    c.PatientId == patientId &&
                    c.Status == ConnectionStatus.Accepted);

            if (connection == null)
                return StatusCode(403, "This patient is not assigned to you!");

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.User)
                .Include(p => p.Patient)
                    .ThenInclude(p => p.User)
                .Include(p => p.Medicines)
                .Where(p => p.PatientId == patientId && p.DoctorId == doctor.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PrescriptionResponseDTO
                {
                    Id = p.Id,
                    PatientId = p.PatientId,
                    PatientName = p.Patient.User.FirstName + " " + p.Patient.User.LastName,
                    DoctorId = p.DoctorId,
                    DoctorName = "Dr. " + p.Doctor.User.FirstName + " " + p.Doctor.User.LastName,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt,
                    Medicines = p.Medicines.Select(m => new MedicineResponseDTO
                    {
                        Id = m.Id,
                        MedicineName = m.MedicineName,
                        Dosage = m.Dosage,
                        TimesPerDay = m.TimesPerDay,
                        Duration = m.Duration
                    }).ToList()
                })
                .ToListAsync();

            return Ok(prescriptions);
        }

        // GET /api/prescription/{id}
        // View a specific prescription
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrescription(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var prescription = await _context.Prescriptions
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.User)
                .Include(p => p.Patient)
                    .ThenInclude(p => p.User)
                .Include(p => p.Medicines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound("Prescription not found!");

            // Make sure only the doctor who wrote it or the patient can view it
            if (role == "Patient")
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null || prescription.PatientId != patient.Id)
                    return StatusCode(403, "Access denied!");
            }
            else if (role == "Doctor")
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null || prescription.DoctorId != doctor.Id)
                    return StatusCode(403, "Access denied!");
            }

            return Ok(new PrescriptionResponseDTO
            {
                Id = prescription.Id,
                PatientId = prescription.PatientId,
                PatientName = prescription.Patient.User.FirstName + " " + prescription.Patient.User.LastName,
                DoctorId = prescription.DoctorId,
                DoctorName = "Dr. " + prescription.Doctor.User.FirstName + " " + prescription.Doctor.User.LastName,
                Notes = prescription.Notes,
                CreatedAt = prescription.CreatedAt,
                Medicines = prescription.Medicines.Select(m => new MedicineResponseDTO
                {
                    Id = m.Id,
                    MedicineName = m.MedicineName,
                    Dosage = m.Dosage,
                    TimesPerDay = m.TimesPerDay,
                    Duration = m.Duration
                }).ToList()
            });
        }

        // DELETE /api/prescription/{id}
        // Doctor deletes a prescription
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can delete prescriptions!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctor.Id);

            if (prescription == null)
                return NotFound("Prescription not found!");

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();

            return Ok("Prescription deleted successfully!");
        }
    }
}