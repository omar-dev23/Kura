namespace Kura.API.Models
{
    public class Message
    {
        public int Id { get; set; }

        // Sender
        public int SenderUserId { get; set; }
        public User Sender { get; set; } = null!;

        // Receiver
        public int ReceiverUserId { get; set; }
        public User Receiver { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}