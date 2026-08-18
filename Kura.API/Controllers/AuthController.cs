using Kura.API.Data;
using Kura.API.DTOs;
using Kura.API.Helpers;
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
    public class AuthController : ControllerBase
    {
        private readonly KuraDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly IEmailService _emailService;

        public AuthController(
            KuraDbContext context,
            JwtHelper jwtHelper,
            IEmailService emailService)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
                return BadRequest("Email already exists!");

          
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber ?? string.Empty,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            if (dto.Role == "Patient")
            {
                user.Patient = new Patient
                {
                    NationalId = dto.NationalId ?? string.Empty,
                    BloodType = dto.BloodType ?? string.Empty,
                    Allergies = dto.Allergies ?? string.Empty,
                    ChronicDiseases = dto.ChronicDiseases ?? string.Empty,
                    Weight = dto.Weight ?? 0,
                    Height = dto.Height ?? 0,
                    Gender = dto.Gender ?? string.Empty,
                    Age = dto.Age ?? 0,
                    EmergencyContact = dto.EmergencyContact ?? string.Empty,
                    Address = dto.Address ?? string.Empty
                };
            }


            if (dto.Role == "Doctor")
            {
                user.Doctor = new Doctor
                {
                    Specialization = dto.Specialization ?? string.Empty,
                    LicenseNumber = dto.LicenseNumber ?? string.Empty,
                    Hospital = dto.Hospital ?? string.Empty,
                    Address = dto.Address ?? string.Empty,
                    NationalId = dto.NationalId ?? string.Empty,
                    Gender = dto.Gender ?? string.Empty,
                    Age = dto.Age ?? 0,
                    IsVerified = false
                };
            }

            if (dto.Role == "Organization")
            {
                if (dto.OrganizationType == null)
                    return BadRequest("Organization type is required!");

                if (string.IsNullOrWhiteSpace(dto.OrganizationName))
                    return BadRequest("Organization name is required!");

                user.Organization = new Organization
                {
                    Type = dto.OrganizationType.Value,
                    Name = dto.OrganizationName,
                    Address = dto.Address ?? string.Empty,
                    PhoneNumber = dto.PhoneNumber ?? string.Empty,
                    LicenseNumber = dto.LicenseNumber ?? string.Empty,
                    WorkingHours = dto.WorkingHours,
                    IsVerified = false
                };
            }


            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, "Account created successfully!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return NotFound("Email not found!");

            if (!user.IsActive)
                return Unauthorized("Account is deactivated!");

            bool passwordCorrect = BCrypt.Net.BCrypt
                .Verify(dto.Password, user.PasswordHash);

            if (!passwordCorrect)
                return Unauthorized("Wrong password!");

            var token = _jwtHelper.GenerateToken(user);

            var response = new AuthResponseDTO
            {
                Token = token,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(response);
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("User not found!");

            bool isCurrentPasswordCorrect = BCrypt.Net.BCrypt
                .Verify(dto.CurrentPassword, user.PasswordHash);

            if (!isCurrentPasswordCorrect)
                return BadRequest("Current password is incorrect!");

            if (dto.CurrentPassword == dto.NewPassword)
                return BadRequest("New password must be different from current password!");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Password changed successfully!");
        }

        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var user = await _context.Users
                .Include(u => u.Patient!)
                    .ThenInclude(p => p.Documents)
                .Include(u => u.Doctor)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found!");

            // ── Doctor: delete all related data ─────────────────────
            if (role == "Doctor" && user.Doctor != null)
            {
                var doctorId = user.Doctor.Id;

                // Delete all connections
                var connections = await _context.DoctorPatientConnections
                    .Where(c => c.DoctorId == doctorId)
                    .ToListAsync();
                _context.DoctorPatientConnections.RemoveRange(connections);

                // Delete certificates
                var certificates = await _context.DoctorCertificates
                    .Where(c => c.DoctorId == doctorId)
                    .ToListAsync();
                _context.DoctorCertificates.RemoveRange(certificates);

                // Delete services
                var services = await _context.DoctorServices
                    .Where(s => s.DoctorId == doctorId)
                    .ToListAsync();
                _context.DoctorServices.RemoveRange(services);

                // Delete workplaces
                var workplaces = await _context.DoctorWorkplaces
                    .Where(w => w.DoctorId == doctorId)
                    .ToListAsync();
                _context.DoctorWorkplaces.RemoveRange(workplaces);

                // Delete prescriptions
                var prescriptions = await _context.Prescriptions
                    .Where(p => p.DoctorId == doctorId)
                    .ToListAsync();
                _context.Prescriptions.RemoveRange(prescriptions);

                // Delete appointments
                var appointments = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId)
                    .ToListAsync();
                _context.Appointments.RemoveRange(appointments);

                await _context.SaveChangesAsync();
            }

            // ── Patient: delete all related data ────────────────────
            if (role == "Patient" && user.Patient != null)
            {
                var patientId = user.Patient.Id;

                // Delete physical files
                if (user.Patient.Documents != null)
                {
                    foreach (var document in user.Patient.Documents)
                    {
                        if (System.IO.File.Exists(document.FilePath))
                            System.IO.File.Delete(document.FilePath);
                    }
                }

                // Delete doctor-patient connections
                var doctorConnections = await _context.DoctorPatientConnections
                    .Where(c => c.PatientId == patientId)
                    .ToListAsync();
                _context.DoctorPatientConnections.RemoveRange(doctorConnections);

                // Delete org connections
                var orgConnections = await _context.PatientOrganizationConnections
                    .Where(c => c.PatientId == patientId)
                    .ToListAsync();
                _context.PatientOrganizationConnections.RemoveRange(orgConnections);

                // Delete prescriptions
                var prescriptions = await _context.Prescriptions
                    .Where(p => p.PatientId == patientId)
                    .ToListAsync();
                _context.Prescriptions.RemoveRange(prescriptions);

                // Delete appointments
                var appointments = await _context.Appointments
                    .Where(a => a.PatientId == patientId)
                    .ToListAsync();
                _context.Appointments.RemoveRange(appointments);

                await _context.SaveChangesAsync();
            }

            // ── Organization: delete all related data ───────────────
            if (role == "Organization" && user.Organization != null)
            {
                var orgId = user.Organization.Id;

                var orgConnections = await _context.PatientOrganizationConnections
                    .Where(c => c.OrganizationId == orgId)
                    .ToListAsync();
                _context.PatientOrganizationConnections.RemoveRange(orgConnections);

                await _context.SaveChangesAsync();
            }

            // ── Delete the user (cascade handles the rest) ───────────
            // ── Delete all messages (sent and received) ──────────────
            var messages = await _context.Messages
                .Where(m => m.SenderUserId == userId || m.ReceiverUserId == userId)
                .ToListAsync();
            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            // ── Delete all notifications ─────────────────────────────
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            // ── Delete the user (cascade handles the rest) ───────────
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Account deleted successfully!");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Ok("If this email exists, an OTP has been sent.");

            var otp = new Random().Next(100000, 999999).ToString();

            user.OtpCode = otp;
            user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendOtpAsync(user.Email, otp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Email sending failed: {ex.Message}");
            }

            return Ok("If this email exists, an OTP has been sent.");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return BadRequest("Invalid request!");

            if (user.OtpCode != dto.Code)
                return BadRequest("Invalid OTP code!");

            if (user.OtpExpiresAt < DateTime.UtcNow)
                return BadRequest("OTP has expired! Please request a new one.");

            var resetToken = Guid.NewGuid().ToString();
            user.ResetToken = resetToken;
            user.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

            user.OtpCode = null;
            user.OtpExpiresAt = null;

            await _context.SaveChangesAsync();

            return Ok(new { resetToken });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return BadRequest("Invalid request!");

            if (user.ResetToken != dto.Token)
                return BadRequest("Invalid or expired reset token!");

            if (user.ResetTokenExpiresAt < DateTime.UtcNow)
                return BadRequest("Reset token has expired! Please start over.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            user.ResetToken = null;
            user.ResetTokenExpiresAt = null;

            await _context.SaveChangesAsync();

            return Ok("Password reset successfully! You can now log in.");
        }
    }
}