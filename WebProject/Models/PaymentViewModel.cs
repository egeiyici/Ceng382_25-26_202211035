using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
    public class PaymentViewModel
    {
        [Required]
        [Display(Name = "Card Holder Name")]
        public string CardHolderName { get; set; } = null!;

        [Required]
        [CreditCard]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; } = null!;

        [Required]
        [Display(Name = "Expiration Date")]
        public string ExpirationDate { get; set; } = null!;

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CVV { get; set; } = null!;

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        public decimal TotalPrice
        {
            get
            {
                return CartItems.Sum(x => x.LineTotal);
            }
        }
    }
}