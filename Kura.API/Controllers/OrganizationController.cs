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
    public class OrganizationController : ControllerBase
    {
        private readonly KuraDbContext _context;

        public OrganizationController(KuraDbContext context)
        {
            _context = context;
        }

        // GET /api/organization/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var org = await _context.Organizations
                .Include(o => o.Services)
                .Include(o => o.Departments)
                .Include(o => o.Specialties)
                .Include(o => o.Pharmacists)
                .Include(o => o.LabDoctors)
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null)
                return NotFound("Organization profile not found!");

            return Ok(MapToProfileDTO(org));
        }

        // PUT /api/organization/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateOrganizationDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null)
                return NotFound("Organization profile not found!");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                org.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Address))
                org.Address = dto.Address;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                org.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                org.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.WorkingHours))
                org.WorkingHours = dto.WorkingHours;
            if (!string.IsNullOrWhiteSpace(dto.LicenseNumber))
                org.LicenseNumber = dto.LicenseNumber;

            if (!string.IsNullOrWhiteSpace(dto.Specialty))
                org.Specialty = dto.Specialty;

            if (dto.TotalDrugs.HasValue)
                org.TotalDrugs = dto.TotalDrugs.Value;

            if (dto.TotalDevices.HasValue)
                org.TotalDevices = dto.TotalDevices.Value;

            await _context.SaveChangesAsync();
            return Ok("Profile updated successfully!");
        }

        // GET /api/organization/all?type=Hospital
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string? type)
        {
            var query = _context.Organizations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(type) &&
                Enum.TryParse<OrganizationType>(type, true, out var orgType))
            {
                query = query.Where(o => o.Type == orgType);
            }

            var orgs = await query
                .Select(o => new OrganizationListDTO
                {
                    Id = o.Id,
                    Name = o.Name,
                    Type = o.Type == OrganizationType.Laboratory ? "Lab" : o.Type.ToString(),
                    Address = o.Address,
                    ProfilePhoto = o.ProfilePhoto,
                    Rating = o.Rating,
                    IsVerified = o.IsVerified
                })
                .ToListAsync();

            return Ok(orgs);
        }

        // GET /api/organization/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var org = await _context.Organizations
                .Include(o => o.Services)
                .Include(o => o.Departments)
                .Include(o => o.Specialties)
                .Include(o => o.Pharmacists)
                .Include(o => o.LabDoctors)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (org == null)
                return NotFound("Organization not found!");

            return Ok(MapToProfileDTO(org));
        }

        // GET /api/organization/search?name=nile&type=Hospital
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(
            [FromQuery] string? name,
            [FromQuery] string? type)
        {
            var query = _context.Organizations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(o => o.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(type) &&
                Enum.TryParse<OrganizationType>(type, true, out var orgType))
                query = query.Where(o => o.Type == orgType);

            var results = await query
                .Select(o => new OrganizationListDTO
                {
                    Id = o.Id,
                    Name = o.Name,
                    Type = o.Type == OrganizationType.Laboratory ? "Lab" : o.Type.ToString(),
                    Address = o.Address,
                    ProfilePhoto = o.ProfilePhoto,
                    Rating = o.Rating,
                    IsVerified = o.IsVerified
                })
                .ToListAsync();

            return Ok(results);
        }

        // POST /api/organization/services
        [HttpPost("services")]
        public async Task<IActionResult> AddService(AddOrgServiceDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null)
                return NotFound("Organization not found!");

            _context.OrganizationServices.Add(new OrganizationService
            {
                OrganizationId = org.Id,
                Name = dto.Name
            });

            await _context.SaveChangesAsync();
            return StatusCode(201, "Service added!");
        }

        // DELETE /api/organization/services/{id}
        [HttpDelete("services/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null)
                return NotFound("Organization not found!");

            var service = await _context.OrganizationServices
                .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == org.Id);

            if (service == null)
                return NotFound("Service not found!");

            _context.OrganizationServices.Remove(service);
            await _context.SaveChangesAsync();
            return Ok("Service deleted!");
        }

        // POST /api/organization/departments
        [HttpPost("departments")]
        public async Task<IActionResult> AddDepartment(AddOrgDepartmentDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null)
                return NotFound("Organization not found!");

            _context.OrganizationDepartments.Add(new OrganizationDepartment
            {
                OrganizationId = org.Id,
                Name = dto.Name
            });

            await _context.SaveChangesAsync();
            return StatusCode(201, "Department added!");
        }

        // DELETE /api/organization/departments/{id}
        [HttpDelete("departments/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null)
                return NotFound("Organization not found!");

            var dept = await _context.OrganizationDepartments
                .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == org.Id);

            if (dept == null)
                return NotFound("Department not found!");

            _context.OrganizationDepartments.Remove(dept);
            await _context.SaveChangesAsync();
            return Ok("Department deleted!");
        }

        // POST /api/organization/rate/{id}
        [HttpPost("rate/{id}")]
        public async Task<IActionResult> Rate(int id, RateOrganizationDTO dto)
        {
            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == id);

            if (org == null)
                return NotFound("Organization not found!");

            var totalRating = org.Rating * org.RatingCount + dto.Rating;
            org.RatingCount++;
            org.Rating = Math.Round(totalRating / org.RatingCount, 1);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Rating submitted!", newRating = org.Rating });
        }

        // Helper
        private static OrganizationProfileDTO MapToProfileDTO(Organization org)
        {
            return new OrganizationProfileDTO
            {
                Id = org.Id,
                Name = org.Name,
                Type = org.Type == OrganizationType.Laboratory ? "Lab" : org.Type.ToString(),
                Address = org.Address,
                PhoneNumber = org.PhoneNumber,
                Description = org.Description,
                ProfilePhoto = org.ProfilePhoto,
                WorkingHours = org.WorkingHours,
                LicenseNumber = org.LicenseNumber,
                Rating = org.Rating,
                RatingCount = org.RatingCount,
                IsVerified = org.IsVerified,
                Specialty = org.Specialty,
                TotalDrugs = org.TotalDrugs,
                TotalDevices = org.TotalDevices,
                Services = org.Services.Select(s => new OrgServiceDTO
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList(),
                Departments = org.Departments.Select(d => new OrgDepartmentDTO
                {
                    Id = d.Id,
                    Name = d.Name
                }).ToList(),
                Specialties = org.Specialties.Select(s => new OrgSpecialtyDTO
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList(),
                Pharmacists = org.Pharmacists.Select(p => new OrgPharmacistDTO
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToList(),
                Doctors = org.LabDoctors.Select(d => new OrgLabDoctorDTO
                {
                    Id = d.Id,
                    Name = d.Name
                }).ToList()
            };
        }
        // POST /api/organization/upload-photo
        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto([FromBody] UploadOrgPhotoDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null)
                return NotFound("Organization not found!");

            if (string.IsNullOrWhiteSpace(dto.Base64Image))
                return BadRequest("Image is required!");

            org.ProfilePhoto = dto.Base64Image;
            await _context.SaveChangesAsync();

            return Ok("Profile photo updated successfully!");
        }

        // POST /api/organization/specialties
        [HttpPost("specialties")]
        public async Task<IActionResult> AddSpecialty(AddOrgSpecialtyDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null) return NotFound("Organization not found!");

            _context.OrganizationSpecialties.Add(new OrganizationSpecialty
            {
                OrganizationId = org.Id,
                Name = dto.Name
            });

            await _context.SaveChangesAsync();
            return StatusCode(201, "Specialty added!");
        }

        // DELETE /api/organization/specialties/{id}
        [HttpDelete("specialties/{id}")]
        public async Task<IActionResult> DeleteSpecialty(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null) return NotFound("Organization not found!");

            var specialty = await _context.OrganizationSpecialties
                .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == org.Id);

            if (specialty == null) return NotFound("Specialty not found!");

            _context.OrganizationSpecialties.Remove(specialty);
            await _context.SaveChangesAsync();
            return Ok("Specialty deleted!");
        }

        // POST /api/organization/pharmacists
        [HttpPost("pharmacists")]
        public async Task<IActionResult> AddPharmacist(AddOrgPharmacistDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null) return NotFound("Organization not found!");

            _context.OrganizationPharmacists.Add(new OrganizationPharmacist
            {
                OrganizationId = org.Id,
                Name = dto.Name
            });

            await _context.SaveChangesAsync();
            return StatusCode(201, "Pharmacist added!");
        }

        // DELETE /api/organization/pharmacists/{id}
        [HttpDelete("pharmacists/{id}")]
        public async Task<IActionResult> DeletePharmacist(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null) return NotFound("Organization not found!");

            var pharmacist = await _context.OrganizationPharmacists
                .FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == org.Id);

            if (pharmacist == null) return NotFound("Pharmacist not found!");

            _context.OrganizationPharmacists.Remove(pharmacist);
            await _context.SaveChangesAsync();
            return Ok("Pharmacist deleted!");
        }

        // POST /api/organization/lab-doctors
        [HttpPost("lab-doctors")]
        public async Task<IActionResult> AddLabDoctor(AddOrgLabDoctorDTO dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null) return NotFound("Organization not found!");

            _context.OrganizationLabDoctors.Add(new OrganizationLabDoctor
            {
                OrganizationId = org.Id,
                Name = dto.Name
            });

            await _context.SaveChangesAsync();
            return StatusCode(201, "Doctor added!");
        }

        // DELETE /api/organization/lab-doctors/{id}
        [HttpDelete("lab-doctors/{id}")]
        public async Task<IActionResult> DeleteLabDoctor(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Organization")
                return StatusCode(403, "Only organizations can access this!");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (org == null) return NotFound("Organization not found!");

            var doctor = await _context.OrganizationLabDoctors
                .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == org.Id);

            if (doctor == null) return NotFound("Doctor not found!");

            _context.OrganizationLabDoctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return Ok("Doctor deleted!");
        }
    }
}