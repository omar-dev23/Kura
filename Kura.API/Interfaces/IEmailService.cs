namespace Kura.API.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otpCode);
    }
}