namespace Movie_Site_Management_System.Services.Service
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;          // TLS
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = "";    // e.g., youraddress@gmail.com
        public string Password { get; set; } = "";    // Gmail App Password
        public string FromEmail { get; set; } = "";   // sender email
        public string FromName { get; set; } = "CINEX";
    }
}
