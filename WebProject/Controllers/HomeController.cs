using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using WebProject.Services;

namespace WebProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GoogleMapsService _googleMapsService;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            GoogleMapsService googleMapsService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _googleMapsService = googleMapsService;
        }

        public async Task<IActionResult> Index()
        {
            var menuItems = await _context.MenuItems
                .Include(m => m.MenuOptions)
                .Include(m => m.Ratings)
                .Include(m => m.Caretaker)
                .ToListAsync();

            var caretakerRatings = _context.CaretakerRatings
                .GroupBy(r => r.CaretakerId)
                .ToDictionary(
                    g => g.Key!,
                    g => g.Average(r => r.Score)
                );

            ViewBag.CaretakerRatings = caretakerRatings;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser?.Latitude != null && currentUser.Longitude != null)
                {
                    var filteredItems = new List<MenuItem>();

                    foreach (var item in menuItems)
                    {
                        if (item.Caretaker?.Latitude == null || item.Caretaker.Longitude == null)
                        {
                            continue;
                        }

                        var distance = await _googleMapsService.GetDistanceInKmAsync(
    currentUser.Latitude.Value,
    currentUser.Longitude.Value,
    item.Caretaker.Latitude.Value,
    item.Caretaker.Longitude.Value);

if (distance == null)
{
    distance = CalculateDistanceInKm(
        currentUser.Latitude.Value,
        currentUser.Longitude.Value,
        item.Caretaker.Latitude.Value,
        item.Caretaker.Longitude.Value);
}

if (distance <= 10)
{
    filteredItems.Add(item);
}
                    }

                    ViewBag.LocationFilterMessage = "Showing restaurants within 10 km using Google Maps API.";
                    return View(filteredItems);
                }
            }

            ViewBag.LocationFilterMessage = "Location data is missing. Showing all menu items.";
            return View(menuItems);
        }
        private double CalculateDistanceInKm(double lat1, double lon1, double lat2, double lon2)
{
    const double earthRadiusKm = 6371;

    var dLat = DegreesToRadians(lat2 - lat1);
    var dLon = DegreesToRadians(lon2 - lon1);

    var a =
        Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        Math.Cos(DegreesToRadians(lat1)) *
        Math.Cos(DegreesToRadians(lat2)) *
        Math.Sin(dLon / 2) *
        Math.Sin(dLon / 2);

    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

    return earthRadiusKm * c;
}

private double DegreesToRadians(double degrees)
{
    return degrees * Math.PI / 180;
}

        public IActionResult Privacy()
        {
            return View();
        }
    }
}