namespace Kura.API.Models
{
    public class OrganizationService
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
    }
}