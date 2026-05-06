using WebProject.Data;
using WebProject.Models;

namespace WebProject.Services
{
    public class LogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(string eventType, string? description, string? userId = null)
        {
            var log = new LogEntry
            {
                EventType = eventType,
                Description = description,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.LogEntries.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}