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
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LogService _logService;
        private readonly EmailService _emailService;

        public OrdersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            LogService logService,
            EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
            _emailService = emailService;
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [Authorize(Roles = "Caretaker")]
        public async Task<IActionResult> CaretakerOrders(string? searchTerm, int page = 1)
        {
            int pageSize = 10;

            var caretakerId = _userManager.GetUserId(User);

            var query = _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.User)
                .Include(oi => oi.MenuItem)
                .Where(oi => oi.MenuItem != null &&
                             oi.MenuItem.CaretakerId == caretakerId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(oi =>
                    oi.OrderId.ToString().Contains(searchTerm) ||
                    (oi.Order != null &&
                     oi.Order.User != null &&
                     oi.Order.User.Email != null &&
                     oi.Order.User.Email.Contains(searchTerm)) ||
                    (oi.MenuItem != null &&
                     oi.MenuItem.Name.Contains(searchTerm)));
            }

            var totalItems = await query.CountAsync();

            var orderItems = await query
                .OrderByDescending(oi => oi.Order!.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages =
                (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(orderItems);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOrders(string? searchTerm, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(o =>
                    o.Id.ToString().Contains(searchTerm) ||
                    o.Status.Contains(searchTerm) ||
                    (o.User != null &&
                     o.User.Email != null &&
                     o.User.Email.Contains(searchTerm)));
            }

            var totalItems = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages =
                (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Caretaker")]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var allowedStatuses = new List<string>
            {
                "Pending Approval",
                "Approved",
                "Preparing",
                "Out for Delivery",
                "Delivered",
                "Cancelled"
            };

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest();
            }

            var caretakerId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var belongsToCaretaker = order.OrderItems.Any(oi =>
                oi.MenuItem != null &&
                oi.MenuItem.CaretakerId == caretakerId);

            if (!belongsToCaretaker)
            {
                return Forbid();
            }

            var oldStatus = order.Status;
            order.Status = status;

            await _context.SaveChangesAsync();
            var customer = await _userManager.FindByIdAsync(order.UserId);

if (customer?.Email != null)
{
    var orderedPackages = string.Join("", order.OrderItems.Select(x =>
        $@"
        <tr>
            <td style='padding:14px;border-bottom:1px solid #e5e7eb;'>
                <strong style='color:#111827;font-size:15px;'>
                    {x.MenuItem?.Name}
                </strong>

                <div style='color:#64748b;font-size:13px;margin-top:4px;'>
                    Guest Count: {x.PersonCount}
                </div>

                <div style='color:#64748b;font-size:13px;margin-top:4px;'>
                    Quantity: {x.Quantity}
                </div>
            </td>

            <td style='padding:14px;border-bottom:1px solid #e5e7eb;text-align:right;font-weight:900;color:#166534;white-space:nowrap;'>
                {x.LineTotal:0.00} ₺
            </td>
        </tr>"
    ));

    string statusColor = status switch
    {
        "Preparing" => "#D97706",
        "Delivered" => "#15803D",
        "Completed" => "#166534",
        "Cancelled" => "#DC2626",
        _ => "#2563EB"
    };

    string statusBg = status switch
    {
        "Preparing" => "#FEF3C7",
        "Delivered" => "#DCFCE7",
        "Completed" => "#DCFCE7",
        "Cancelled" => "#FEE2E2",
        _ => "#DBEAFE"
    };

    var htmlMail =
    $@"
<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;background:#f3f4f6;font-family:Arial,sans-serif;color:#111827;'>

    <div style='max-width:700px;margin:0 auto;padding:28px;'>

        <div style='background:linear-gradient(135deg,#0f172a,#1e293b);padding:32px;border-radius:24px 24px 0 0;color:white;'>

            <div style='font-size:13px;font-weight:800;letter-spacing:.08em;text-transform:uppercase;color:#cbd5e1;'>
                Food4Everyone Catering Platform
            </div>

            <h1 style='margin:14px 0 10px 0;font-size:32px;line-height:1.1;'>
                Catering Order Status Updated
            </h1>

            <p style='margin:0;color:#e2e8f0;font-size:15px;line-height:1.7;'>
                Your catering order has been updated by the catering company.
            </p>

        </div>

        <div style='background:white;padding:28px;border-left:1px solid #e5e7eb;border-right:1px solid #e5e7eb;'>

            <div style='display:flex;gap:14px;flex-wrap:wrap;margin-bottom:22px;'>

                <div style='background:#eff6ff;border:1px solid #bfdbfe;border-radius:18px;padding:16px;min-width:180px;'>
                    <div style='font-size:12px;font-weight:800;color:#64748b;text-transform:uppercase;'>
                        Order Number
                    </div>

                    <div style='font-size:24px;font-weight:900;color:#0f172a;margin-top:4px;'>
                        #{order.Id}
                    </div>
                </div>

                <div style='background:{statusBg};border-radius:18px;padding:16px;min-width:220px;border:1px solid rgba(0,0,0,0.05);'>
                    <div style='font-size:12px;font-weight:800;color:{statusColor};text-transform:uppercase;'>
                        Current Status
                    </div>

                    <div style='font-size:22px;font-weight:900;color:{statusColor};margin-top:4px;'>
                        {status}
                    </div>
                </div>

            </div>

            <div style='background:#f8fafc;border:1px solid #e2e8f0;border-radius:20px;padding:20px;margin-bottom:24px;'>

                <h2 style='margin:0 0 12px 0;color:#0f172a;font-size:20px;'>
                    Order Summary
                </h2>

                <p style='margin:6px 0;color:#475569;'>
                    <strong>Order Date:</strong>
                    {order.CreatedAt:dd.MM.yyyy HH:mm}
                </p>

                <p style='margin:6px 0;color:#475569;'>
                    <strong>Total Price:</strong>
                    {order.TotalPrice:0.00} ₺
                </p>

            </div>

            <table style='width:100%;border-collapse:collapse;border:1px solid #e5e7eb;border-radius:18px;overflow:hidden;'>

                <thead>
                    <tr style='background:#0f172a;color:white;'>
                        <th style='padding:14px;text-align:left;font-size:13px;'>
                            Catering Package
                        </th>

                        <th style='padding:14px;text-align:right;font-size:13px;'>
                            Total
                        </th>
                    </tr>
                </thead>

                <tbody>
                    {orderedPackages}
                </tbody>

            </table>

            <div style='margin-top:24px;padding:18px;background:#f8fafc;border:1px solid #e2e8f0;border-radius:18px;'>

                <strong style='color:#0f172a;font-size:16px;'>
                    Status Information
                </strong>

                <p style='margin:8px 0 0 0;color:#475569;line-height:1.7;'>
                    You can track your order anytime from the “My Orders” page on Food4Everyone.
                </p>

            </div>

        </div>

        <div style='background:#0f172a;color:#cbd5e1;padding:18px 26px;border-radius:0 0 24px 24px;text-align:center;font-size:13px;'>
            Food4Everyone © 2026 — Smart Catering Platform
        </div>

    </div>

</body>
</html>";

    _emailService.SendOrderEmail(
        customer.Email,
        $"Your Catering Order #{order.Id} Status Updated",
        htmlMail);
}

            await _logService.AddLogAsync(
                "Catering Order Status Updated",
                $"Catering order #{order.Id} status changed from {oldStatus} to {status}.",
                caretakerId);

            
            return RedirectToAction(nameof(CaretakerOrders));
        }
    }
}