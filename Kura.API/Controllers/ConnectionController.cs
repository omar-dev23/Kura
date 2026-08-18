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
    public class ConnectionController : ControllerBase
    {
        private readonly KuraDbContext _context;
        private readonly INotificationService _notificationService;

        public ConnectionController(
            KuraDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchDoctors(
            [FromQuery] string? name,
            [FromQuery] string? specialization)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Patient")
                return StatusCode(403, "Only patients can search for doctors!");

            var query = _context.Doctors
                .Include(d => d.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(d =>
                    d.User.FirstName.Contains(name) ||
                    d.User.LastName.Contains(name));

            if (!string.IsNullOrWhiteSpace(specialization))
                query = query.Where(d =>
                    d.Specialization.Contains(specialization));

            var doctors = await query
                .Select(d => new SearchDoctorDTO
                {
                    Id = d.Id,
                    FullName = d.User.FirstName + " " + d.User.LastName,
                    Specialization = d.Specialization,
                    Hospital = d.Hospital,
                    IsVerified = d.IsVerified
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpPost("send-request")]
        public async Task<IActionResult> SendRequest(SendConnectionRequestDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Patient")
                return StatusCode(403, "Only patients can send connection requests!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient profile not found!");

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

            if (doctor == null)
                return NotFound("Doctor not found!");

            var existingRequest = await _context.DoctorPatientConnections
                .FirstOrDefaultAsync(c =>
                    c.PatientId == patient.Id &&
                    c.DoctorId == dto.DoctorId);

            if (existingRequest != null)
            {
                if (existingRequest.Status == ConnectionStatus.Pending)
                    return BadRequest("You already have a pending request with this doctor!");

                if (existingRequest.Status == ConnectionStatus.Accepted)
                    return BadRequest("You are already connected with this doctor!");

                if (existingRequest.Status == ConnectionStatus.Rejected)
                {
                    existingRequest.Status = ConnectionStatus.Pending;
                    existingRequest.RequestedAt = DateTime.UtcNow;
                    existingRequest.RespondedAt = null;
                    await _context.SaveChangesAsync();

                    await _notificationService.SendAsync(
                        doctor.UserId,
                        "New Connection Request",
                        $"Patient {patient.Id} has sent you a new connection request!");

                    return Ok("Connection request sent again!");
                }
            }

            var connection = new DoctorPatientConnection
            {
                PatientId = patient.Id,
                DoctorId = dto.DoctorId,
                Status = ConnectionStatus.Pending
            };

            _context.DoctorPatientConnections.Add(connection);
            await _context.SaveChangesAsync();

            await _notificationService.SendAsync(
                doctor.UserId,
                "New Connection Request",
                $"A patient has sent you a connection request!");

            return Ok("Connection request sent successfully!");
        }

        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can view pending requests!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var requests = await _context.DoctorPatientConnections
                .Include(c => c.Patient)
                    .ThenInclude(p => p.User)
                .Where(c => c.DoctorId == doctor.Id &&
                            c.Status == ConnectionStatus.Pending)
                .Select(c => new ConnectionResponseDTO
                {
                    Id = c.Id,
                    Status = c.Status.ToString(),
                    RequestedAt = c.RequestedAt,
                    RespondedAt = c.RespondedAt,
                    Patient = new PatientInfoDTO
                    {
                        Id = c.Patient.Id,
                        UserId = c.Patient.UserId,
                        FullName = c.Patient.User.FirstName + " " + c.Patient.User.LastName,
                        BloodType = c.Patient.BloodType,
                        Age = c.Patient.Age,
                        Gender = c.Patient.Gender,
                        ProfilePhoto = c.Patient.ProfilePhoto
                    }
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPut("{id}/respond")]
        public async Task<IActionResult> RespondToRequest(int id, [FromQuery] string action)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can respond to requests!");

            if (action != "accept" && action != "reject")
                return BadRequest("Action must be 'accept' or 'reject'!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var connection = await _context.DoctorPatientConnections
                .Include(c => c.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.DoctorId == doctor.Id &&
                    c.Status == ConnectionStatus.Pending);

            if (connection == null)
                return NotFound("Connection request not found!");

            connection.Status = action == "accept"
                ? ConnectionStatus.Accepted
                : ConnectionStatus.Rejected;

            connection.RespondedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var doctorUser = await _context.Users.FindAsync(userId);
            var message = action == "accept"
                ? $"Dr. {doctorUser!.FirstName} {doctorUser.LastName} accepted your connection request!"
                : $"Dr. {doctorUser!.FirstName} {doctorUser.LastName} rejected your connection request.";

            await _notificationService.SendAsync(
                connection.Patient.UserId,
                action == "accept" ? "Request Accepted" : "Request Rejected",
                message);

            return Ok($"Request {action}ed successfully!");
        }

        [HttpGet("myconnections")]
        public async Task<IActionResult> GetMyConnections()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Patient")
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                    return NotFound("Patient profile not found!");

                var connections = await _context.DoctorPatientConnections
                    .Include(c => c.Doctor)
                        .ThenInclude(d => d.User)
                    .Where(c => c.PatientId == patient.Id &&
                                c.Status == ConnectionStatus.Accepted)
                    .Select(c => new ConnectionResponseDTO
                    {
                        Id = c.Id,
                        Status = c.Status.ToString(),
                        RequestedAt = c.RequestedAt,
                        RespondedAt = c.RespondedAt,
                        Doctor = new DoctorInfoDTO
                        {
                            Id = c.Doctor.Id,
                            UserId = c.Doctor.UserId,
                            FullName = c.Doctor.User.FirstName + " " + c.Doctor.User.LastName,
                            Specialization = c.Doctor.Specialization,
                            Hospital = c.Doctor.Hospital,
                            IsVerified = c.Doctor.IsVerified,
                            ProfilePhoto = c.Doctor.ProfilePhoto
                        }
                    })
                    .ToListAsync();

                return Ok(connections);
            }
            else if (role == "Doctor")
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                    return NotFound("Doctor profile not found!");

                var connections = await _context.DoctorPatientConnections
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .Where(c => c.DoctorId == doctor.Id &&
                                c.Status == ConnectionStatus.Accepted)
                    .Select(c => new ConnectionResponseDTO
                    {
                        Id = c.Id,
                        Status = c.Status.ToString(),
                        RequestedAt = c.RequestedAt,
                        RespondedAt = c.RespondedAt,
                        Patient = new PatientInfoDTO
                        {
                            Id = c.Patient.Id,
                            UserId = c.Patient.UserId,
                            FullName = c.Patient.User.FirstName + " " + c.Patient.User.LastName,
                            BloodType = c.Patient.BloodType,
                            Age = c.Patient.Age,
                            Gender = c.Patient.Gender,
                            ProfilePhoto = c.Patient.ProfilePhoto
                        }
                    })
                    .ToListAsync();

                return Ok(connections);
            }

            return BadRequest("Invalid role!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveConnection(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            DoctorPatientConnection? connection = null;

            if (role == "Patient")
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                    return NotFound("Patient profile not found!");

                connection = await _context.DoctorPatientConnections
                    .Include(c => c.Doctor)
                        .ThenInclude(d => d.User)
                    .FirstOrDefaultAsync(c =>
                        c.Id == id && c.PatientId == patient.Id);
            }
            else if (role == "Doctor")
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                    return NotFound("Doctor profile not found!");

                connection = await _context.DoctorPatientConnections
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(c =>
                        c.Id == id && c.DoctorId == doctor.Id);
            }

            if (connection == null)
                return NotFound("Connection not found!");

            _context.DoctorPatientConnections.Remove(connection);
            await _context.SaveChangesAsync();

            if (role == "Patient")
            {
                await _notificationService.SendAsync(
                    connection.Doctor.UserId,
                    "Connection Removed",
                    "A patient has removed their connection with you.");
            }
            else
            {
                await _notificationService.SendAsync(
                    connection.Patient.UserId,
                    "Connection Removed",
                    "A doctor has removed their connection with you.");
            }

            return Ok("Connection removed successfully!");
        }
    }
}