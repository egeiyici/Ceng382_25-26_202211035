using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebProject.Services
{
    public class FileEmailSender : IEmailSender
    {
        private readonly IWebHostEnvironment _environment;

        public FileEmailSender(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task SendEmailAsync(
            string email,
            string subject,
            string htmlMessage)
        {
            var logFolder =
                Path.Combine(_environment.ContentRootPath, "EmailLogs");

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            var fileName =
                $"{DateTime.Now:yyyyMMdd_HHmmss}_{email.Replace("@", "_at_")}.txt";

            var filePath = Path.Combine(logFolder, fileName);

            var content = $"""
TO: {email}

SUBJECT:
{subject}

MESSAGE:
{htmlMessage}
""";

            await File.WriteAllTextAsync(filePath, content);
        }
    }
}