using Kura.API.Data;
using Kura.API.Interfaces;
using Kura.API.Models;

namespace Kura.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly KuraDbContext _context;

        public NotificationService(KuraDbContext context)
        {
            _context = context;
        }

        public async Task SendAsync(int userId, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}