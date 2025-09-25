using System.Net;
using System.Net.Mail;
using Movie_Site_Management_System.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Movie_Site_Management_System.Services.Service
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _opt;
        public SmtpEmailService(IOptions<SmtpOptions> opt) => _opt = opt.Value;

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            byte[]? attachmentBytes = null,
            string? attachmentName = null,
            string? mimeType = "application/pdf",
            CancellationToken ct = default)
        {
            using var msg = new MailMessage
            {
                From = new MailAddress(_opt.FromEmail, _opt.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(new MailAddress(toEmail));

            if (attachmentBytes != null && !string.IsNullOrWhiteSpace(attachmentName))
            {
                var stream = new MemoryStream(attachmentBytes);
                var attachment = new Attachment(stream, attachmentName, mimeType ?? "application/octet-stream");
                msg.Attachments.Add(attachment);
            }

            using var client = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.EnableSsl,
                Credentials = new NetworkCredential(_opt.Username, _opt.Password)
            };

            await client.SendMailAsync(msg);
        }
    }
}
