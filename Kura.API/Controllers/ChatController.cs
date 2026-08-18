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
    public class ChatController : ControllerBase
    {
        private readonly KuraDbContext _context;

        public ChatController(KuraDbContext context)
        {
            _context = context;
        }

        // POST /api/chat/send
        // Send a message
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(SendMessageDTO dto)
        {
            var senderUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var senderRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Can't send to yourself
            if (senderUserId == dto.ReceiverUserId)
                return BadRequest("You can't send a message to yourself!");

            // Check receiver exists
            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.ReceiverUserId);

            if (receiver == null)
                return NotFound("Receiver not found!");

            // Validate allowed chat combinations
            // Patient ↔ Doctor or Patient ↔ Organization only
            var allowedRoles = new[] { "Patient", "Doctor", "Organization" };

            if (!allowedRoles.Contains(senderRole))
                return StatusCode(403, "Your role cannot send messages!");

            if (!allowedRoles.Contains(receiver.Role))
                return StatusCode(403, "You cannot send messages to this user!");

            // Patient can chat with Doctor or Organization
            // Doctor can chat with Patient
            // Organization can chat with Patient
            if (senderRole == "Doctor" && receiver.Role != "Patient")
                return StatusCode(403, "Doctors can only message patients!");

            if (senderRole == "Organization" && receiver.Role != "Patient")
                return StatusCode(403, "Organizations can only message patients!");

            if (senderRole == "Patient" &&
                receiver.Role != "Doctor" &&
                receiver.Role != "Organization")
                return StatusCode(403, "Patients can only message doctors or organizations!");

            var message = new Message
            {
                SenderUserId = senderUserId,
                ReceiverUserId = dto.ReceiverUserId,
                Content = dto.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                Message = "Message sent!",
                MessageId = message.Id,
                SentAt = message.SentAt
            });
        }

        // GET /api/chat/conversations
        // Get list of all conversations
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Get all users this person has chatted with
            var sentTo = await _context.Messages
                .Where(m => m.SenderUserId == userId)
                .Select(m => m.ReceiverUserId)
                .Distinct()
                .ToListAsync();

            var receivedFrom = await _context.Messages
                .Where(m => m.ReceiverUserId == userId)
                .Select(m => m.SenderUserId)
                .Distinct()
                .ToListAsync();

            var contactUserIds = sentTo.Union(receivedFrom).Distinct().ToList();

            var conversations = new List<ConversationDTO>();

            foreach (var contactId in contactUserIds)
            {
                var contact = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == contactId);

                if (contact == null) continue;

                // Get last message
                var lastMessage = await _context.Messages
                    .Where(m => (m.SenderUserId == userId && m.ReceiverUserId == contactId) ||
                                (m.SenderUserId == contactId && m.ReceiverUserId == userId))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync();

                // Get unread count
                var unreadCount = await _context.Messages
                    .CountAsync(m => m.SenderUserId == contactId &&
                                     m.ReceiverUserId == userId &&
                                     !m.IsRead);

                // Get profile photo
                string? photo = null;
                if (contact.Role == "Doctor")
                {
                    var doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.UserId == contactId);
                    photo = doctor?.ProfilePhoto;
                }
                else if (contact.Role == "Patient")
                {
                    var patient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.UserId == contactId);
                    photo = patient?.ProfilePhoto;
                }
                else if (contact.Role == "Organization")
                {
                    var org = await _context.Organizations
                        .FirstOrDefaultAsync(o => o.UserId == contactId);
                    photo = org?.ProfilePhoto;
                }

                conversations.Add(new ConversationDTO
                {
                    UserId = contactId,
                    Name = contact.FirstName + " " + contact.LastName,
                    Role = contact.Role,
                    ProfilePhoto = photo,
                    LastMessage = lastMessage?.Content ?? "",
                    LastMessageAt = lastMessage?.SentAt ?? DateTime.UtcNow,
                    UnreadCount = unreadCount
                });
            }

            // Sort by last message
            conversations = conversations
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            return Ok(conversations);
        }

        // GET /api/chat/messages/{otherUserId}
        // Get all messages between current user and another user
        [HttpGet("messages/{otherUserId}")]
        public async Task<IActionResult> GetMessages(int otherUserId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Check other user exists
            var otherUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == otherUserId);

            if (otherUser == null)
                return NotFound("User not found!");

            // Get all messages between the two users
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderUserId == userId && m.ReceiverUserId == otherUserId) ||
                            (m.SenderUserId == otherUserId && m.ReceiverUserId == userId))
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageResponseDTO
                {
                    Id = m.Id,
                    SenderUserId = m.SenderUserId,
                    SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                    SenderRole = m.Sender.Role,
                    ReceiverUserId = m.ReceiverUserId,
                    ReceiverName = m.Receiver.FirstName + " " + m.Receiver.LastName,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    SentAt = m.SentAt,
                    IsMine = m.SenderUserId == userId
                })
                .ToListAsync();

            // Mark all received messages as read
            var unreadMessages = await _context.Messages
                .Where(m => m.SenderUserId == otherUserId &&
                            m.ReceiverUserId == userId &&
                            !m.IsRead)
                .ToListAsync();

            unreadMessages.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();

            return Ok(messages);
        }

        // DELETE /api/chat/messages/{id}
        // Delete a specific message (only sender can delete)
        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id && m.SenderUserId == userId);

            if (message == null)
                return NotFound("Message not found!");

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok("Message deleted!");
        }

        // GET /api/chat/unread-count
        // Get total unread messages count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var count = await _context.Messages
                .CountAsync(m => m.ReceiverUserId == userId && !m.IsRead);

            return Ok(new { unreadCount = count });
        }
    }
}