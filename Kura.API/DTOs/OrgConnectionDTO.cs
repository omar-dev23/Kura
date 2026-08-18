using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class SendOrgConnectionRequestDTO
    {
        [Required(ErrorMessage = "OrganizationId is required!")]
        public int OrganizationId { get; set; }
    }

    public class OrgConnectionResponseDTO
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public OrgInfoDTO? Organization { get; set; }
        public OrgPatientInfoDTO? Patient { get; set; }
    }

    public class OrgInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public double Rating { get; set; }
        public bool IsVerified { get; set; }
    }

    public class OrgPatientInfoDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
    }
}