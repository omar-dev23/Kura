using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class SendMessageDTO
    {
        [Required(ErrorMessage = "Receiver is required!")]
        public int ReceiverUserId { get; set; }

        [Required(ErrorMessage = "Message content is required!")]
        [StringLength(1000, MinimumLength = 1,
            ErrorMessage = "Message must be between 1 and 1000 characters!")]
        public string Content { get; set; } = string.Empty;
    }

    public class MessageResponseDTO
    {
        public int Id { get; set; }
        public int SenderUserId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public int ReceiverUserId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsMine { get; set; }
    }

    public class ConversationDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}