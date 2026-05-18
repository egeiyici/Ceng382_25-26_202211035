namespace WebProject.Models
{
    public class RestaurantViewModel
    {
        public string CaretakerId { get; set; } = null!;

        public string RestaurantName { get; set; } = null!;

        public string? Address { get; set; }

        public double AverageRating { get; set; }

        public int MenuItemCount { get; set; }

        public double? DistanceKm { get; set; }
    }
}