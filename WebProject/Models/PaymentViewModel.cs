using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
    public class PaymentViewModel
    {
        [Required]
        [Display(Name = "Card Holder Name")]
        public string CardHolderName { get; set; } = null!;

       [Required]
[RegularExpression(@"^\d{16}$",
    ErrorMessage = "Card number must be 16 digits.")]
[Display(Name = "Card Number")]
public string CardNumber { get; set; } = null!;
        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$",
            ErrorMessage = "Expiration date must be in MM/YY format.")]
        [Display(Name = "Expiration Date")]
        public string ExpirationDate { get; set; } = null!;

        [Required]
        [StringLength(3,
            MinimumLength = 3,
            ErrorMessage = "CVV must be 3 digits.")]
        [RegularExpression(@"^[0-9]{3}$",
            ErrorMessage = "CVV must contain only numbers.")]
        [Display(Name = "CVV")]
        public string CVV { get; set; } = null!;

        public List<CartItem> CartItems { get; set; }
            = new List<CartItem>();

        public decimal TotalPrice
        {
            get
            {
                return CartItems.Sum(x => x.LineTotal);
            }
        }
    }
}