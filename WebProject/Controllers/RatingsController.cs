using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using WebProject.Services;

namespace WebProject.Controllers
{
    [Authorize(Roles = "User")]
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LogService _logService;

        public RatingsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            LogService logService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
        }

        public async Task<IActionResult> Create(int orderId)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.Status == "Completed");

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int orderId,
            int menuItemId,
            int menuScore,
            string? menuComment,
            string caretakerId,
            int caretakerScore,
            string? caretakerComment)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.Status == "Completed");

            if (order == null)
            {
                return NotFound();
            }

            var orderItem = order.OrderItems.FirstOrDefault(x => x.MenuItemId == menuItemId);

            if (orderItem == null || orderItem.MenuItem == null)
            {
                return NotFound();
            }

            var menuRating = new MenuItemRating
            {
                OrderId = orderId,
                MenuItemId = menuItemId,
                UserId = userId,
                Score = menuScore,
                Comment = menuComment,
                CreatedAt = DateTime.Now
            };

            var caretakerRating = new CaretakerRating
            {
                OrderId = orderId,
                CaretakerId = caretakerId,
                UserId = userId,
                Score = caretakerScore,
                Comment = caretakerComment,
                CreatedAt = DateTime.Now
            };

            _context.MenuItemRatings.Add(menuRating);
            _context.CaretakerRatings.Add(caretakerRating);

            await _context.SaveChangesAsync();

            await _logService.AddLogAsync(
                "Rating Submitted",
                $"User rated menu item #{menuItemId} and caretaker #{caretakerId}.",
                userId);

            return RedirectToAction("Index", "Home");
        }
    }
}