using Microsoft.EntityFrameworkCore;

namespace WebProject.Models
{
    public class OrderItemOption
    {
        public int Id { get; set; }

        public int OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

        public int MenuOptionId { get; set; }
        public MenuOption? MenuOption { get; set; }

        public string OptionName { get; set; } = null!;

        [Precision(18, 2)]
        public decimal ExtraPrice { get; set; }
    }
}