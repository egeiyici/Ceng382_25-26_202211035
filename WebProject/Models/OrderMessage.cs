using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
    public class OrderMessage
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public string SenderId { get; set; } = null!;
        public ApplicationUser? Sender { get; set; }

        [Required]
        public string MessageText { get; set; } = null!;

        public DateTime SentAt { get; set; }
    }
}