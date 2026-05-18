using System.Net;
using System.Net.Mail;
using System.Text;

namespace WebProject.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendOrderEmail(string to, string subject, string content)
        {
            try
            {
                var smtpHost = _configuration["SmtpSettings:Host"];
                var smtpPort = int.Parse(_configuration["SmtpSettings:Port"]!);
                var smtpUser = _configuration["SmtpSettings:UserName"];
                var smtpPassword = _configuration["SmtpSettings:Password"];
                var fromEmail = _configuration["SmtpSettings:FromEmail"];
                var fromName = _configuration["SmtpSettings:FromName"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = content,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "EmailLogs");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}_FAILED_EMAIL.txt";
                var fullPath = Path.Combine(folderPath, fileName);

                var builder = new StringBuilder();

                builder.AppendLine("EMAIL SEND FAILED");
                builder.AppendLine("ERROR: " + ex.Message);
                builder.AppendLine("--------------------------------");
                builder.AppendLine("TO: " + to);
                builder.AppendLine("SUBJECT: " + subject);
                builder.AppendLine("--------------------------------");
                builder.AppendLine(content);

                File.WriteAllText(fullPath, builder.ToString());
            }
        }
    }
}