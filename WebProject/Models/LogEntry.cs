namespace WebProject.Models
{
    public class LogEntry
    {
        public int Id { get; set; }

        public string EventType { get; set; } = null!;

        public string? Description { get; set; }

        public string? UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}