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
    public class OrgConnectionController : ControllerBase
    {
        private readonly KuraDbContext _context;
        private readonly INotificationService _notificationService;

        public OrgConnectionController(
            KuraDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET /api/orgconnection/all-organizations
        // Patient views all organizations
        [HttpGet("all-organizations")]
        public async Task<IActionResult> GetAllOrganizations(
            [FromQuery] string? type,
            [FromQuery] string? name)
        {
            var query = _context.Organizations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(type) &&
                Enum.TryParse<OrganizationType>(type, true, out var orgType))
                query = query.Where(o => o.Type == orgType);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(o => o.Name.Contains(name));

            var orgs = await query
                .Select(o => new OrgInfoDTO
                {
                    Id = o.Id,
                    Name = o.Name,
                    Type = o.Type == OrganizationType.Laboratory
                        ? "Lab" : o.Type.ToString(),
                    Address = o.Address,
                    ProfilePhoto = o.ProfilePhoto,
                    Rating = o.Rating,
                    IsVerified = o.IsVerified
                })
                .ToListAsync();

            return Ok(orgs);
        }

        // POST /api/orgconnection/send-request
        // Patient sends a connection request to an organization
        [HttpPost("send-request")]
        public async Task<IActionResult> SendRequest(SendOrgConnectionRequestDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Patient")
                return StatusCode(403, "Only patients can send connection requests!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient profile not found!");

            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId);

            if (org == null)
                return NotFound("Organization not found!");

            // Check if request already exists
            var existing = await _context.PatientOrganizationConnections
                .FirstOrDefaultAsync(c =>
                    c.PatientId == patient.Id &&
                    c.OrganizationId == dto.OrganizationId);

            if (existing != null)
            {
                if (existing.Status == OrgConnectionStatus.Pending)
                    return BadRequest("You already have a pending request with this organization!");

                if (existing.Status == OrgConnectionStatus.Accepted)
                    return BadRequest("You are already connected with this organization!");

                // If rejected → allow resending
                if (existing.Status == OrgConnectionStatus.Rejected)
                {
                    existing.Status = OrgConnectionStatus.Pending;
                    existing.RequestedAt = DateTime.UtcNow;
                    existing.RespondedAt = null;
                    await _context.SaveChangesAsync();

                    await _notificationService.SendAsync(
                        org.UserId,
                        "New Connection Request",
                        $"A patient has sent you a new connection request!");

                    return Ok("Connection request sent again!");
                }
            }

            // Create new request
            var connection = new PatientOrganizationConnection
            {
                PatientId = patient.Id,
                OrganizationId = dto.OrganizationId,
                Status = OrgConnectionStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.PatientOrganizationConnections.Add(connection);
            await _context.SaveChangesAsync();

            // Notify the organization
            await _notificationService.SendAsync(
                org.UserId,
                "New Connection Request",
                $"{patient.User.FirstName} {patient.User.LastName} sent you a connection request!");

            return StatusCode(201, "Connection request sent successfully!");
        }

        // GET /api/orgconnection/pending-requests
        // Organization views all pending requests
        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can view pending requests!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null)
                return NotFound("Organization not found!");

            var requests = await _context.PatientOrganizationConnections
                .Include(c => c.Patient)
                    .ThenInclude(p => p.User)
                .Where(c => c.OrganizationId == org.Id &&
                            c.Status == OrgConnectionStatus.Pending)
                .Select(c => new OrgConnectionResponseDTO
                {
                    Id = c.Id,
                    Status = c.Status.ToString(),
                    RequestedAt = c.RequestedAt,
                    RespondedAt = c.RespondedAt,
                    Patient = new OrgPatientInfoDTO
                    {
                        Id = c.Patient.Id,
                        UserId = c.Patient.UserId,
                        FullName = c.Patient.User.FirstName + " " + c.Patient.User.LastName,
                        ProfilePhoto = c.Patient.ProfilePhoto,
                        Age = c.Patient.Age,
                        Gender = c.Patient.Gender,
                        BloodType = c.Patient.BloodType
                    }
                })
                .ToListAsync();

            return Ok(requests);
        }

        // PUT /api/orgconnection/{id}/respond?action=accept
        // Organization accepts or rejects a request
        [HttpPut("{id}/respond")]
        public async Task<IActionResult> RespondToRequest(
            int id,
            [FromQuery] string action)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can respond to requests!");

            if (action != "accept" && action != "reject")
                return BadRequest("Action must be 'accept' or 'reject'!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null)
                return NotFound("Organization not found!");

            var connection = await _context.PatientOrganizationConnections
                .Include(c => c.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.OrganizationId == org.Id &&
                    c.Status == OrgConnectionStatus.Pending);

            if (connection == null)
                return NotFound("Connection request not found!");

            connection.Status = action == "accept"
                ? OrgConnectionStatus.Accepted
                : OrgConnectionStatus.Rejected;

            connection.RespondedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify the patient
            var message = action == "accept"
                ? $"{org.Name} accepted your connection request!"
                : $"{org.Name} rejected your connection request.";

            await _notificationService.SendAsync(
                connection.Patient.UserId,
                action == "accept" ? "Request Accepted" : "Request Rejected",
                message);

            return Ok($"Request {action}ed successfully!");
        }

        // GET /api/orgconnection/my-connections
        // Patient views their connected organizations
        // Organization views their connected patients
        [HttpGet("my-connections")]
        public async Task<IActionResult> GetMyConnections()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Patient")
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                    return NotFound("Patient not found!");

                var connections = await _context.PatientOrganizationConnections
                    .Include(c => c.Organization)
                    .Where(c => c.PatientId == patient.Id &&
                                c.Status == OrgConnectionStatus.Accepted)
                    .Select(c => new OrgConnectionResponseDTO
                    {
                        Id = c.Id,
                        Status = c.Status.ToString(),
                        RequestedAt = c.RequestedAt,
                        RespondedAt = c.RespondedAt,
                        Organization = new OrgInfoDTO
                        {
                            Id = c.Organization.Id,
                            Name = c.Organization.Name,
                            Type = c.Organization.Type == OrganizationType.Laboratory
                                ? "Lab" : c.Organization.Type.ToString(),
                            Address = c.Organization.Address,
                            ProfilePhoto = c.Organization.ProfilePhoto,
                            Rating = c.Organization.Rating,
                            IsVerified = c.Organization.IsVerified
                        }
                    })
                    .ToListAsync();

                return Ok(connections);
            }
            else if (role == "Organization")
            {
                var org = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.UserId == userId);

                if (org == null)
                    return NotFound("Organization not found!");

                var connections = await _context.PatientOrganizationConnections
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .Where(c => c.OrganizationId == org.Id &&
                                c.Status == OrgConnectionStatus.Accepted)
                    .Select(c => new OrgConnectionResponseDTO
                    {
                        Id = c.Id,
                        Status = c.Status.ToString(),
                        RequestedAt = c.RequestedAt,
                        RespondedAt = c.RespondedAt,
                        Patient = new OrgPatientInfoDTO
                        {
                            Id = c.Patient.Id,
                            UserId = c.Patient.UserId,
                            FullName = c.Patient.User.FirstName + " " + c.Patient.User.LastName,
                            ProfilePhoto = c.Patient.ProfilePhoto,
                            Age = c.Patient.Age,
                            Gender = c.Patient.Gender,
                            BloodType = c.Patient.BloodType
                        }
                    })
                    .ToListAsync();

                return Ok(connections);
            }

            return StatusCode(403, "Access denied!");
        }

        // DELETE /api/orgconnection/{id}
        // Both patient and organization can remove a connection
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveConnection(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            PatientOrganizationConnection? connection = null;

            if (role == "Patient")
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                    return NotFound("Patient not found!");

                connection = await _context.PatientOrganizationConnections
                    .Include(c => c.Organization)
                    .FirstOrDefaultAsync(c =>
                        c.Id == id && c.PatientId == patient.Id);
            }
            else if (role == "Organization")
            {
                var org = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.UserId == userId);

                if (org == null)
                    return NotFound("Organization not found!");

                connection = await _context.PatientOrganizationConnections
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(c =>
                        c.Id == id && c.OrganizationId == org.Id);
            }

            if (connection == null)
                return NotFound("Connection not found!");

            _context.PatientOrganizationConnections.Remove(connection);
            await _context.SaveChangesAsync();

            // Notify the other party
            if (role == "Patient" && connection.Organization != null)
            {
                await _notificationService.SendAsync(
                    connection.Organization.UserId,
                    "Connection Removed",
                    "A patient has removed their connection with you.");
            }
            else if (role == "Organization" && connection.Patient != null)
            {
                await _notificationService.SendAsync(
                    connection.Patient.UserId,
                    "Connection Removed",
                    "An organization has removed their connection with you.");
            }

            return Ok("Connection removed successfully!");
        }
    }
}