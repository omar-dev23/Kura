using Kura.API.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Kura.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpAsync(string toEmail, string otpCode)
        {
            var fromEmail = _config["EmailSettings:FromEmail"]!;
            var fromName = _config["EmailSettings:FromName"]!;
            var smtpHost = _config["EmailSettings:SmtpHost"]!;
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]!);
            var username = _config["EmailSettings:Username"]!;
            var password = _config["EmailSettings:Password"]!;

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(fromName, fromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Kura — Your Password Reset Code";

            email.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 400px; margin: auto;'>
                    <h2 style='color: #2c3e50;'>Password Reset Request</h2>
                    <p>You requested to reset your Kura account password.</p>
                    <p>Your verification code is:</p>
                    <div style='font-size: 36px; font-weight: bold; 
                                letter-spacing: 8px; color: #2980b9;
                                padding: 20px; background: #f0f4f8;
                                text-align: center; border-radius: 8px;'>
                        {otpCode}
                    </div>
                    <p style='color: #e74c3c;'>This code expires in <strong>5 minutes</strong>.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                </div>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}