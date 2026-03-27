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

    public void SendOTScheduleUpdated(
        string toEmail,
        string employeeName,
        string scheduleName,
        DateTime fromDate, TimeSpan fromTime,
        DateTime toDate, TimeSpan toTime,
        int quantity,
        string description)
    {
        var subject = $"[Thông Báo] Lịch tăng ca \"{scheduleName}\" đã được cập nhật";

        var descRow = !string.IsNullOrWhiteSpace(description)
            ? $@"<tr style=""background:#f8fafc;"">
                   <td style=""padding:8px 0;color:#64748b;"">Mô tả</td>
                   <td style=""padding:8px 0;color:#475569;"">{description}</td>
                 </tr>"
            : string.Empty;

        var body = $@"
<html>
<body style=""font-family:Arial,sans-serif;color:#333;max-width:600px;margin:0 auto;"">
  <div style=""background:linear-gradient(135deg,#3b82f6,#6366f1);padding:28px 32px;border-radius:12px 12px 0 0;"">
    <h1 style=""color:white;margin:0;font-size:20px;"">&#x1F4CB; Lịch Tăng Ca Đã Được Cập Nhật</h1>
  </div>
  <div style=""background:#f8fafc;padding:28px 32px;border:1px solid #e2e8f0;border-top:none;border-radius:0 0 12px 12px;"">
    <p>Xin chào <strong>{employeeName}</strong>,</p>
    <p style=""color:#475569;"">Người quản lý của bạn vừa cập nhật thông tin lịch tăng ca mà bạn đang tham gia. Vui lòng xem lại thông tin bên dưới:</p>
    <div style=""background:white;border:1px solid #e2e8f0;border-radius:10px;padding:20px;margin:20px 0;"">
      <h2 style=""margin:0 0 16px;font-size:16px;color:#1e293b;"">&#x1F5D3; {scheduleName}</h2>
      <table style=""width:100%;border-collapse:collapse;font-size:14px;"">
        <tr>
          <td style=""padding:8px 0;color:#64748b;width:160px;"">Thời gian bắt đầu</td>
          <td style=""padding:8px 0;font-weight:600;color:#1e293b;"">{fromTime:hh\:mm} ngày {fromDate:dd\/MM\/yyyy}</td>
        </tr>
        <tr style=""background:#f8fafc;"">
          <td style=""padding:8px 0;color:#64748b;"">Thời gian kết thúc</td>
          <td style=""padding:8px 0;font-weight:600;color:#1e293b;"">{toTime:hh\:mm} ngày {toDate:dd\/MM\/yyyy}</td>
        </tr>
        <tr>
          <td style=""padding:8px 0;color:#64748b;"">Số lượng người</td>
          <td style=""padding:8px 0;font-weight:600;color:#1e293b;"">{quantity} người</td>
        </tr>
        {descRow}
      </table>
    </div>
    <p style=""font-size:13px;color:#94a3b8;"">Email này được gửi tự động từ hệ thống HR. Vui lòng không trả lời.</p>
  </div>
</body>
</html>";

        var mail = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mail.To.Add(toEmail);

        var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = true
            };

            client.Send(mail);
        }

        public void SendOTScheduleCancelled(
            string toEmail,
            string employeeName,
            string scheduleName,
            DateTime fromDate, TimeSpan fromTime,
            DateTime toDate, TimeSpan toTime)
        {
            var subject = $"[Thông Báo] Lịch tăng ca \"{scheduleName}\" đã bị HỦY";

            var body = $@"
    <html>
    <body style=""font-family:Arial,sans-serif;color:#333;max-width:600px;margin:0 auto;"">
      <div style=""background:linear-gradient(135deg,#ef4444,#b91c1c);padding:28px 32px;border-radius:12px 12px 0 0;"">
        <h1 style=""color:white;margin:0;font-size:20px;"">&#x274C; Lịch Tăng Ca Đã Bị Hủy</h1>
      </div>
      <div style=""background:#f8fafc;padding:28px 32px;border:1px solid #e2e8f0;border-top:none;border-radius:0 0 12px 12px;"">
        <p>Xin chào <strong>{employeeName}</strong>,</p>
        <p style=""color:#475569;"">Người quản lý của bạn vừa thông báo <strong>HỦY</strong> lịch tăng ca mà bạn đang tham gia. Dưới đây là thông tin lịch đã hủy:</p>
        <div style=""background:white;border:1px solid #e2e8f0;border-radius:10px;padding:20px;margin:20px 0;"">
          <h2 style=""margin:0 0 16px;font-size:16px;color:#1e293b;text-decoration:line-through;"">&#x1F5D3; {scheduleName}</h2>
          <table style=""width:100%;border-collapse:collapse;font-size:14px;"">
            <tr>
              <td style=""padding:8px 0;color:#64748b;width:160px;"">Thời gian bắt đầu</td>
              <td style=""padding:8px 0;font-weight:600;color:#1e293b;"">{fromTime:hh\:mm} ngày {fromDate:dd\/MM\/yyyy}</td>
            </tr>
            <tr style=""background:#f8fafc;"">
              <td style=""padding:8px 0;color:#64748b;"">Thời gian kết thúc</td>
              <td style=""padding:8px 0;font-weight:600;color:#1e293b;"">{toTime:hh\:mm} ngày {toDate:dd\/MM\/yyyy}</td>
            </tr>
          </table>
        </div>
        <p style=""color:#64748b;font-size:14px;margin-top:24px;border-top:1px solid #e2e8f0;padding-top:24px;"">
          Hệ thống Quản lý Nhân sự<br>
          <em>Email này được tạo tự động, vui lòng không trả lời.</em>
        </p>
      </div>
    </body>
    </html>";

            var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(
                    _settings.Username,
                    _settings.Password
                ),
                EnableSsl = true
            };

            client.Send(mail);
        }

        public void SendOTRegistrationRemoved(
            string toEmail,
            string employeeName,
            string scheduleName,
            DateTime fromDate, TimeSpan fromTime,
            DateTime toDate, TimeSpan toTime)
        {
            var subject = $"[Thông Báo] Bạn đã bị xóa khỏi lịch tăng ca \"{scheduleName}\"";

            var body = $@"
<html>
<body style=""font-family:Arial,sans-serif;color:#333;max-width:600px;margin:0 auto;"">
  <div style=""background:linear-gradient(135deg,#ef4444,#b91c1c);padding:28px 32px;border-radius:12px 12px 0 0;"">
    <h1 style=""color:white;margin:0;font-size:20px;"">&#x274C; Thông báo cập nhật tham gia OT</h1>
  </div>
  <div style=""background:#f8fafc;padding:28px 32px;border:1px solid #e2e8f0;border-top:none;border-radius:0 0 12px 12px;"">
    <p>Xin chào <strong>{employeeName}</strong>,</p>
    <p style=""color:#475569;"">Quản lý đã xóa bạn khỏi danh sách tham gia lịch tăng ca dưới đây:</p>
    <div style=""background:white;border:1px solid #e2e8f0;border-radius:10px;padding:20px;margin:20px 0;"">
      <h2 style=""margin:0 0 16px;font-size:16px;color:#1e293b;"">&#x1F5D3; {scheduleName}</h2>
      <table style=""width:100%;border-collapse:collapse;font-size:14px;"">
        <tr>
          <td style=""padding:8px 0;color:#64748b;width:160px;"">Thời gian bắt đầu</td>
          <td style=""padding:8px 0;font-weight:600;color:#1e293b;"">{fromTime:hh\:mm} ngày {fromDate:dd\/MM\/yyyy}</td>
        </tr>
        <tr style=""background:#f8fafc;"">
          <td style=""padding:8px 0;color:#64748b;"">Thời gian kết thúc</td>
          <td style=""padding:8px 0;font-weight:600;color:#1e293b;"">{toTime:hh\:mm} ngày {toDate:dd\/MM\/yyyy}</td>
        </tr>
      </table>
    </div>
    <p style=""font-size:13px;color:#94a3b8;"">Email này được gửi tự động từ hệ thống HR. Vui lòng không trả lời.</p>
  </div>
</body>
</html>";

            var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            client.Send(mail);
        }
    }
