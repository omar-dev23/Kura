namespace Kura.API.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(int userId, string title, string message);
    }
}