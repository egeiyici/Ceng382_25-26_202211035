using Microsoft.EntityFrameworkCore;

namespace WebProject.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; }

        [Precision(18, 2)]
        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "Pending";

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}