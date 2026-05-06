using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
    public class MenuItemRating
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Score { get; set; }

        public string? Comment { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}