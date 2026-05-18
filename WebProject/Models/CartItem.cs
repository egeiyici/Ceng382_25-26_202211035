namespace WebProject.Models
{
    public class CartItem
    {
        public int MenuItemId { get; set; }

        public string MenuItemName { get; set; } = null!;

        public decimal BasePrice { get; set; }

        public int MinimumPeople { get; set; }

        public decimal PricePerExtraPerson { get; set; }

        public int PersonCount { get; set; }

        public int Quantity { get; set; }

        public List<int> SelectedOptionIds { get; set; } = new List<int>();

        public List<string> SelectedOptionNames { get; set; } = new List<string>();

        public decimal CustomizationTotal { get; set; }

        public decimal UnitPrice
        {
            get { return BasePrice; }
            set { BasePrice = value; }
        }

        public decimal PricePerPerson
        {
            get { return PricePerExtraPerson; }
            set { PricePerExtraPerson = value; }
        }

        public decimal PackageBaseTotal
        {
            get
            {
                if (PersonCount <= MinimumPeople)
                {
                    return BasePrice;
                }

                int extraPeople = PersonCount - MinimumPeople;

                decimal multiplier = PersonCount switch
                {
                    <= 20 => 1.00m,
                    <= 50 => 0.85m,
                    _ => 0.70m
                };

                return BasePrice + (extraPeople * PricePerExtraPerson * multiplier);
            }
        }

        public decimal LineTotal
        {
            get
            {
                return (PackageBaseTotal + CustomizationTotal) * Quantity;
            }
        }
    }
}