namespace WebProject.Models
{
    public class CartItem
    {
        public int MenuItemId { get; set; }

        public string MenuItemName { get; set; } = null!;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public List<int> SelectedOptionIds { get; set; } = new List<int>();

        public List<string> SelectedOptionNames { get; set; } = new List<string>();

        public decimal CustomizationTotal { get; set; }

        public decimal LineTotal
        {
            get
            {
                return (UnitPrice + CustomizationTotal) * Quantity;
            }
        }
    }
}