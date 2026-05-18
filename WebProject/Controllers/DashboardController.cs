using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Admin));
            }

            if (User.IsInRole("Caretaker"))
            {
                return RedirectToAction(nameof(Caretaker));
            }

            if (User.IsInRole("User"))
            {
                return RedirectToAction(nameof(UserDashboard));
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var allUsers = await _context.Users.ToListAsync();

            int totalUsers = 0;
            int totalCaretakers = 0;
            int totalAdmins = 0;

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "User"))
                {
                    totalUsers++;
                }

                if (await _userManager.IsInRoleAsync(user, "Caretaker"))
                {
                    totalCaretakers++;
                }

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    totalAdmins++;
                }
            }

            var latestOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalCaretakers = totalCaretakers;
            ViewBag.TotalAdmins = totalAdmins;
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalPrice);
            ViewBag.TotalLogs = await _context.LogEntries.CountAsync();
            ViewBag.TotalMenuItems = await _context.MenuItems.CountAsync();
            ViewBag.LatestOrders = latestOrders;

            return View();
        }

        [Authorize(Roles = "Caretaker")]
        public async Task<IActionResult> Caretaker()
        {
            var caretakerId = _userManager.GetUserId(User);

            var menuItemIds = await _context.MenuItems
                .Where(m => m.CaretakerId == caretakerId)
                .Select(m => m.Id)
                .ToListAsync();

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.User)
                .Include(oi => oi.MenuItem)
                .Where(oi => menuItemIds.Contains(oi.MenuItemId))
                .ToListAsync();

            var relatedOrderIds = orderItems
                .Select(oi => oi.OrderId)
                .Distinct()
                .ToList();

            var recentMessages = await _context.OrderMessages
                .Include(m => m.Sender)
                .Where(m => relatedOrderIds.Contains(m.OrderId))
                .OrderByDescending(m => m.SentAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalReceivedOrders = relatedOrderIds.Count;

            ViewBag.CompletedOrders = orderItems
                .Where(oi => oi.Order != null && oi.Order.Status == "Completed")
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();

            ViewBag.TotalRevenue = orderItems.Sum(oi => oi.LineTotal);

            ViewBag.AverageRating = await _context.CaretakerRatings
                .Where(r => r.CaretakerId == caretakerId)
                .AnyAsync()
                    ? await _context.CaretakerRatings
                        .Where(r => r.CaretakerId == caretakerId)
                        .AverageAsync(r => r.Score)
                    : 0;

            ViewBag.MenuItemCount = menuItemIds.Count;
            ViewBag.RecentMessages = recentMessages;
            ViewBag.RecentOrderItems = orderItems
                .OrderByDescending(oi => oi.Order?.CreatedAt)
                .Take(5)
                .ToList();

            return View();
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> UserDashboard()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.TotalPurchases = orders.Count;
            ViewBag.TotalSpent = orders.Sum(o => o.TotalPrice);
            ViewBag.LastOrder = orders.FirstOrDefault();
            ViewBag.RecentOrders = orders.Take(5).ToList();

            ViewBag.AverageGivenMenuRating = await _context.MenuItemRatings
                .Where(r => r.UserId == userId)
                .AnyAsync()
                    ? await _context.MenuItemRatings
                        .Where(r => r.UserId == userId)
                        .AverageAsync(r => r.Score)
                    : 0;

            return View();
        }
    }
}