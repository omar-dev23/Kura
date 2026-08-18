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
    public class AppointmentController : ControllerBase
    {
        private readonly KuraDbContext _context;
        private readonly INotificationService _notificationService;

        public AppointmentController(
            KuraDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // POST /api/appointment
        [HttpPost]
        public async Task<IActionResult> CreateAppointment(CreateAppointmentDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Patient")
                return StatusCode(403, "Only patients can create appointments!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient profile not found!");

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

            if (doctor == null)
                return NotFound("Doctor not found!");

            if (dto.AppointmentDate.Date < DateTime.UtcNow.Date)
                return BadRequest("Appointment date must be in the future!");

            var appointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = patient.Id,
                AppointmentDate = dto.AppointmentDate,
                AppointmentTime = dto.AppointmentTime,
                Notes = dto.Notes,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            await _notificationService.SendAsync(
                doctor.UserId,
                "New Appointment",
                $"{patient.User.FirstName} {patient.User.LastName} booked an appointment on {dto.AppointmentDate:dd/MM/yyyy} at {dto.AppointmentTime}!");

            return StatusCode(201, new
            {
                Message = "Appointment created successfully!",
                AppointmentId = appointment.Id
            });
        }

        // GET /api/appointment/my-appointments
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Patient")
                return StatusCode(403, "Only patients can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient profile not found!");

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.PatientId == patient.Id)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentResponseDTO
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = "Dr. " + a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                    DoctorPhoto = a.Doctor.ProfilePhoto,
                    DoctorSpecialization = a.Doctor.Specialization,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.User.FirstName + " " + a.Patient.User.LastName,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET /api/appointment/doctor-home
        [HttpGet("doctor-home")]
        public async Task<IActionResult> GetDoctorHome()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var now = DateTime.Now;
            var todayStart = now.Date;
            var todayEnd = todayStart.AddDays(1);

            var todayAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id &&
                a.AppointmentDate >= now &&
                a.AppointmentDate < todayEnd)
                .ToListAsync();

            var totalPatients = await _context.DoctorPatientConnections
                .CountAsync(c => c.DoctorId == doctor.Id &&
                                 c.Status == ConnectionStatus.Accepted);

            var upcoming = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id &&
            (a.Status == AppointmentStatus.Pending ||
             a.Status == AppointmentStatus.Confirmed) &&
            a.AppointmentDate >= now)
                .OrderBy(a => a.AppointmentDate)
                .Take(10)
                .Select(a => new AppointmentResponseDTO
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = "Dr. " + a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                    DoctorPhoto = a.Doctor.ProfilePhoto,
                    DoctorSpecialization = a.Doctor.Specialization,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.User.FirstName + " " + a.Patient.User.LastName,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new DoctorHomeStatsDTO
            {
                TotalPatients = totalPatients,
                Done = todayAppointments.Count(a => a.Status == AppointmentStatus.Done),
                Remaining = todayAppointments.Count(a => a.Status == AppointmentStatus.Pending ||
                                                         a.Status == AppointmentStatus.Confirmed),
                UpcomingAppointments = upcoming
            });
        }

        // PUT /api/appointment/{id}/confirm
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can confirm appointments!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null)
                return NotFound("Appointment not found!");

            if (appointment.Status != AppointmentStatus.Pending)
                return BadRequest("Only pending appointments can be confirmed!");

            appointment.Status = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();

            await _notificationService.SendAsync(
                appointment.Patient.UserId,
                "Appointment Confirmed",
                $"Your appointment on {appointment.AppointmentDate:dd/MM/yyyy} at {appointment.AppointmentTime} has been confirmed by the doctor!");

            return Ok(new { Message = "Appointment confirmed successfully!" });
        }

        // PUT /api/appointment/{id}/refuse
        [HttpPut("{id}/refuse")]
        public async Task<IActionResult> RefuseAppointment(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can refuse appointments!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null)
                return NotFound("Appointment not found!");

            if (appointment.Status != AppointmentStatus.Pending)
                return BadRequest("Only pending appointments can be refused!");

            appointment.Status = AppointmentStatus.Refused;
            await _context.SaveChangesAsync();

            await _notificationService.SendAsync(
                appointment.Patient.UserId,
                "Appointment Refused",
                $"Your appointment on {appointment.AppointmentDate:dd/MM/yyyy} at {appointment.AppointmentTime} was declined by the doctor.");

            return Ok(new { Message = "Appointment refused successfully!" });
        }

        // PUT /api/appointment/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateAppointmentStatusDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can update appointment status!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null)
                return NotFound("Appointment not found!");

            appointment.Status = dto.Status == "Done"
                ? AppointmentStatus.Done
                : AppointmentStatus.Cancelled;

            await _context.SaveChangesAsync();

            await _notificationService.SendAsync(
                appointment.Patient.UserId,
                dto.Status == "Done" ? "Appointment Completed" : "Appointment Cancelled",
                $"Your appointment on {appointment.AppointmentDate:dd/MM/yyyy} at {appointment.AppointmentTime} has been marked as {dto.Status}.");

            return Ok($"Appointment marked as {dto.Status}!");
        }

        // GET /api/appointment/doctor-appointments
        // Doctor views ALL their appointments (all statuses, no limit)
        [HttpGet("doctor-appointments")]
        public async Task<IActionResult> GetDoctorAppointments()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentResponseDTO
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = "Dr. " + a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                    DoctorPhoto = a.Doctor.ProfilePhoto,
                    DoctorSpecialization = a.Doctor.Specialization,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.User.FirstName + " " + a.Patient.User.LastName,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // DELETE /api/appointment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Patient")
                return StatusCode(403, "Only patients can cancel appointments!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient profile not found!");

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patient.Id);

            if (appointment == null)
                return NotFound("Appointment not found!");

            if (appointment.Status == AppointmentStatus.Done)
                return BadRequest("Cannot cancel a completed appointment!");

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            await _notificationService.SendAsync(
                appointment.Doctor.UserId,
                "Appointment Cancelled",
                $"A patient cancelled their appointment on {appointment.AppointmentDate:dd/MM/yyyy} at {appointment.AppointmentTime}.");

            return Ok("Appointment cancelled successfully!");
        }

    }
}