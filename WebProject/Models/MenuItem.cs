using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebProject.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Precision(18, 2)]
        public decimal Price { get; set; }

        [Required]
        public string Description { get; set; } = null!;

        public string? CaretakerId { get; set; }
        public ApplicationUser? Caretaker { get; set; }
    }
}