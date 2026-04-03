using System.ComponentModel.DataAnnotations;

namespace Northwind.Mvc.Models
{
    public class ShippersInfo
    {
        [Key]
        public int Id { get; set; }
        public int ShipperId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
    }
}