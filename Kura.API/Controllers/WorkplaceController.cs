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
    public class WorkplaceController : ControllerBase
    {
        private readonly KuraDbContext _context;

        public WorkplaceController(KuraDbContext context)
        {
            _context = context;
        }

        // GET /api/workplace/my-workplaces
        // Doctor views their workplaces
        [HttpGet("my-workplaces")]
        public async Task<IActionResult> GetMyWorkplaces()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var workplaces = await _context.DoctorWorkplaces
                .Include(w => w.Organization)
                .Where(w => w.DoctorId == doctor.Id)
                .Select(w => new WorkplaceResponseDTO
                {
                    Id = w.Id,
                    Name = w.Organization != null
                        ? w.Organization.Name
                        : w.ManualName ?? "Unknown",
                    Address = w.Organization != null
                        ? w.Organization.Address
                        : null,
                    ProfilePhoto = w.Organization != null
                        ? w.Organization.ProfilePhoto
                        : null,
                    Type = w.Organization != null
                        ? w.Organization.Type.ToString()
                        : null,
                    IsLinkedToOrganization = w.OrganizationId != null,
                    AddedAt = w.AddedAt
                })
                .ToListAsync();

            return Ok(workplaces);
        }

        // POST /api/workplace/add
        // Doctor adds a workplace
        [HttpPost("add")]
        public async Task<IActionResult> AddWorkplace(AddWorkplaceDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            // Must provide either OrganizationId or ManualName
            if (dto.OrganizationId == null && string.IsNullOrWhiteSpace(dto.ManualName))
                return BadRequest("Please provide either an OrganizationId or a workplace name!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            // If linking to existing organization, check it exists
            if (dto.OrganizationId != null)
            {
                var orgExists = await _context.Organizations
                    .AnyAsync(o => o.Id == dto.OrganizationId);

                if (!orgExists)
                    return NotFound("Organization not found!");

                // Check not already added
                var alreadyExists = await _context.DoctorWorkplaces
                    .AnyAsync(w => w.DoctorId == doctor.Id &&
                                   w.OrganizationId == dto.OrganizationId);

                if (alreadyExists)
                    return BadRequest("This workplace is already added!");
            }

            var workplace = new DoctorWorkplace
            {
                DoctorId = doctor.Id,
                OrganizationId = dto.OrganizationId,
                ManualName = dto.ManualName,
                AddedAt = DateTime.UtcNow
            };

            _context.DoctorWorkplaces.Add(workplace);
            await _context.SaveChangesAsync();

            return StatusCode(201, "Workplace added successfully!");
        }

        // DELETE /api/workplace/{id}
        // Doctor removes a workplace
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveWorkplace(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Doctor")
                return StatusCode(403, "Only doctors can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found!");

            var workplace = await _context.DoctorWorkplaces
                .FirstOrDefaultAsync(w => w.Id == id && w.DoctorId == doctor.Id);

            if (workplace == null)
                return NotFound("Workplace not found!");

            _context.DoctorWorkplaces.Remove(workplace);
            await _context.SaveChangesAsync();

            return Ok("Workplace removed successfully!");
        }
    }
}