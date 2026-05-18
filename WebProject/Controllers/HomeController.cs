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
                .Include(m => m.Caretaker)
                .ToListAsync();

            var allCateringCompanies = menuItems
                .Where(m => m.CaretakerId != null && m.Caretaker != null)
                .GroupBy(m => new
                {
                    m.CaretakerId,
                    m.Caretaker!.FullName,
                    m.Caretaker.Email,
                    m.Caretaker.Address,
                    m.Caretaker.Latitude,
                    m.Caretaker.Longitude
                })
                .Select(g => new RestaurantViewModel
                {
                    CaretakerId = g.Key.CaretakerId!,
                    RestaurantName = !string.IsNullOrWhiteSpace(g.Key.FullName)
                        ? g.Key.FullName!
                        : g.Key.Email!,
                    Address = g.Key.Address,
                    MenuItemCount = g.Count(),
                    AverageRating = _context.CaretakerRatings
                        .Where(r => r.CaretakerId == g.Key.CaretakerId)
                        .Any()
                            ? _context.CaretakerRatings
                                .Where(r => r.CaretakerId == g.Key.CaretakerId)
                                .Average(r => r.Score)
                            : 0
                })
                .OrderBy(c => c.RestaurantName)
                .ToList();

            var nearbyCateringCompanies =
                new List<RestaurantViewModel>();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser?.Latitude != null &&
                    currentUser.Longitude != null)
                {
                    foreach (var company in allCateringCompanies)
                    {
                        var caretaker =
                            await _userManager.FindByIdAsync(company.CaretakerId);

                        if (caretaker?.Latitude == null ||
                            caretaker.Longitude == null)
                        {
                            continue;
                        }

                        var distance =
                            await _googleMapsService.GetDistanceInKmAsync(
                                currentUser.Latitude.Value,
                                currentUser.Longitude.Value,
                                caretaker.Latitude.Value,
                                caretaker.Longitude.Value);

                        if (distance == null)
                        {
                            distance = CalculateDistanceInKm(
                                currentUser.Latitude.Value,
                                currentUser.Longitude.Value,
                                caretaker.Latitude.Value,
                                caretaker.Longitude.Value);
                        }

                        company.DistanceKm = distance;

                        if (distance <= 10)
                        {
                            nearbyCateringCompanies.Add(company);
                        }
                    }

                    nearbyCateringCompanies =
                        nearbyCateringCompanies
                            .OrderBy(c => c.DistanceKm)
                            .ToList();

                    ViewBag.LocationFilterMessage =
                        "Showing catering companies within 10 km of your selected delivery address.";
                }
                else
                {
                    ViewBag.LocationFilterMessage =
                        "Delivery location is missing. Add your address to see nearby catering companies.";
                }
            }
            else
            {
                ViewBag.LocationFilterMessage =
                    "Login and add a delivery address to see nearby catering companies.";
            }

            ViewBag.AllCateringCompanies = allCateringCompanies;

            return View(nearbyCateringCompanies);
        }

        public async Task<IActionResult> Restaurant(string caretakerId)
        {
            var caretaker = await _userManager.FindByIdAsync(caretakerId);

            if (caretaker == null)
            {
                return NotFound();
            }

            var menuItems = await _context.MenuItems
                .Include(m => m.MenuOptions)
                .Include(m => m.Ratings)
                .Include(m => m.Caretaker)
                .Where(m => m.CaretakerId == caretakerId)
                .ToListAsync();

            var caretakerRatings = _context.CaretakerRatings
                .GroupBy(r => r.CaretakerId)
                .ToDictionary(
                    g => g.Key!,
                    g => g.Average(r => r.Score)
                );

            ViewBag.CaretakerRatings = caretakerRatings;

            ViewBag.RestaurantName = !string.IsNullOrWhiteSpace(caretaker.FullName)
                ? caretaker.FullName
                : caretaker.Email;

            return View(menuItems);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        private double CalculateDistanceInKm(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
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

            var c = 2 * Math.Atan2(
                Math.Sqrt(a),
                Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}