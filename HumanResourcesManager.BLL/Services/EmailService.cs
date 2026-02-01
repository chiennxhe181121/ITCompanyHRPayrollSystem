using HumanResourcesManager.BLL.DTOs;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public void SendOtp(string toEmail, string otp)
    {
        var mail = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = "Password Reset OTP",
            Body = $"Your OTP is: {otp}",
            IsBodyHtml = false
        };

        mail.To.Add(toEmail);

        var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
        {
            Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password
            ),
            EnableSsl = true
        };

        client.Send(mail);
    }
}
