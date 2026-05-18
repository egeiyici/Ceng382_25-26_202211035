using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebProject.Models
{
    public class MenuOption
    {
        public int Id { get; set; }

        [Required]
        public string OptionName { get; set; } = null!;

        [Precision(18, 2)]
        public decimal ExtraPrice { get; set; }

        public bool IsPerPerson { get; set; } = true;

        public int MenuItemId { get; set; }

        public MenuItem? MenuItem { get; set; }
    }
}