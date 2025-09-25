namespace Movie_Site_Management_System.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            byte[]? attachmentBytes = null,
            string? attachmentName = null,
            string? mimeType = null,
            CancellationToken ct = default);
    }
}
