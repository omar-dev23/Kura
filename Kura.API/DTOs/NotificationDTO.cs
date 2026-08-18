using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNotificationDTO
    {
        [Required(ErrorMessage = "UserId is required!")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Title is required!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters!")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required!")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Message must be between 2 and 500 characters!")]
        public string Message { get; set; } = string.Empty;
    }
}