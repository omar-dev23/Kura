using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class AddWorkplaceDTO
    {
        // Either OrganizationId OR ManualName must be provided
        public int? OrganizationId { get; set; }
        public string? ManualName { get; set; }
    }

    public class WorkplaceResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? Type { get; set; }
        public bool IsLinkedToOrganization { get; set; }
        public DateTime AddedAt { get; set; }
    }
}