using Microsoft.AspNetCore.Mvc;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var menuItems = _context.MenuItems.ToList();
            return View(menuItems);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}