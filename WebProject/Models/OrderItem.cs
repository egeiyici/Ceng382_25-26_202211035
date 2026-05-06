using Microsoft.EntityFrameworkCore;

namespace WebProject.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int Quantity { get; set; }

        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }

        [Precision(18, 2)]
        public decimal CustomizationTotal { get; set; }

        [Precision(18, 2)]
        public decimal LineTotal { get; set; }

        public ICollection<OrderItemOption> SelectedOptions { get; set; } = new List<OrderItemOption>();
    }
}