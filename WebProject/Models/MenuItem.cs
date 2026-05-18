using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebProject.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Precision(18, 2)]
        public decimal BasePrice { get; set; }

        public int MinimumPeople { get; set; }

        [Precision(18, 2)]
        public decimal PricePerExtraPerson { get; set; }

        [NotMapped]
        public decimal Price
        {
            get { return BasePrice; }
            set { BasePrice = value; }
        }

        [NotMapped]
        public decimal PricePerPerson
        {
            get { return PricePerExtraPerson; }
            set { PricePerExtraPerson = value; }
        }

        public string? Description { get; set; }

        public string? CaretakerId { get; set; }

        public ApplicationUser? Caretaker { get; set; }

        public byte[]? ImageData { get; set; }

        public string? ImageContentType { get; set; }

        public ICollection<MenuOption> MenuOptions { get; set; } = new List<MenuOption>();

        public ICollection<MenuItemRating>? Ratings { get; set; }
    }
}