using System.Text;

namespace WebProject.Services
{
    public class EmailService
    {
        public void SendOrderEmail(string to, string subject, string content)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "EmailLogs");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.txt";
            var fullPath = Path.Combine(folderPath, fileName);

            var builder = new StringBuilder();

            builder.AppendLine("TO: " + to);
            builder.AppendLine("SUBJECT: " + subject);
            builder.AppendLine("--------------------------------");
            builder.AppendLine(content);

            File.WriteAllText(fullPath, builder.ToString());
        }
    }
}