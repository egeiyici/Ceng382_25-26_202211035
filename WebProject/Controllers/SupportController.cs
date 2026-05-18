using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using WebProject.Services;

namespace WebProject.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LogService _logService;

        public SupportController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            LogService logService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
        }

        public async Task<IActionResult> Chat(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var messages = await _context.OrderMessages
                .Include(m => m.Sender)
                .Where(m => m.OrderId == orderId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.OrderId = orderId;

            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int orderId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return RedirectToAction(nameof(Chat), new { orderId });
            }

            var userId = _userManager.GetUserId(User);

            var message = new OrderMessage
            {
                OrderId = orderId,
                SenderId = userId!,
                MessageText = messageText,
                SentAt = DateTime.Now
            };

            _context.OrderMessages.Add(message);
            await _context.SaveChangesAsync();

            await _logService.AddLogAsync(
                "Support Message Sent",
                $"Support message sent for order #{orderId}.",
                userId);

            return RedirectToAction(nameof(Chat), new { orderId });
        }
    }
}