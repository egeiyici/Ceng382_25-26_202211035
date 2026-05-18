using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Helpers;
using WebProject.Models;
using WebProject.Services;

namespace WebProject.Controllers
{
    [Authorize(Roles = "User")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _logService;
        private readonly EmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        private const string CartSessionKey = "Cart";

        public PaymentController(
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

        public async Task<IActionResult> Checkout()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null ||
                string.IsNullOrWhiteSpace(currentUser.Address))
            {
                TempData["AddressWarning"] =
                    "Please add your delivery address before checkout.";

                return RedirectToAction("Index", "Addresses");
            }

            var cart =
                HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey)
                ?? new List<CartItem>();

            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var viewModel = new PaymentViewModel
            {
                CartItems = cart
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(PaymentViewModel model)
        {
            var cart =
                HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey)
                ?? new List<CartItem>();

            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            model.CartItems = cart;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                Status = "Pending Approval",
                TotalPrice = cart.Sum(x => x.LineTotal)
            };

            foreach (var cartItem in cart)
            {
                var orderItem = new OrderItem
                {
                    MenuItemId = cartItem.MenuItemId,
                    Quantity = cartItem.Quantity,
                    PersonCount = cartItem.PersonCount,
                    MinimumPeople = cartItem.MinimumPeople,
                    UnitPrice = cartItem.UnitPrice,
                    PricePerPerson = cartItem.PricePerPerson,
                    PackageBaseTotal = cartItem.PackageBaseTotal,
                    CustomizationTotal = cartItem.CustomizationTotal,
                    LineTotal = cartItem.LineTotal
                };

                for (int i = 0; i < cartItem.SelectedOptionIds.Count; i++)
                {
                    orderItem.SelectedOptions.Add(new OrderItemOption
                    {
                        MenuOptionId = cartItem.SelectedOptionIds[i],
                        OptionName = cartItem.SelectedOptionNames[i],
                        ExtraPrice = 0
                    });
                }

                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _logService.AddLogAsync(
                "Catering Order Created",
                $"Catering order #{order.Id} created successfully for {cart.Sum(x => x.PersonCount * x.Quantity)} total guests.",
                userId);

            var packageRows = string.Join("", cart.Select(x =>
                $@"
                <tr>
                    <td style='padding:14px;border-bottom:1px solid #e5e7eb;'>
                        <strong style='color:#111827;font-size:15px;'>{x.MenuItemName}</strong>
                        <div style='color:#6b7280;font-size:13px;margin-top:4px;'>
                            Guest Count: {x.PersonCount}
                        </div>
                        <div style='color:#6b7280;font-size:13px;margin-top:4px;'>
                            Features: {(x.SelectedOptionNames.Any() ? string.Join(", ", x.SelectedOptionNames) : "No additional features")}
                        </div>
                    </td>
                    <td style='padding:14px;border-bottom:1px solid #e5e7eb;text-align:right;font-weight:800;color:#166534;white-space:nowrap;'>
                        {x.LineTotal:0.00} ₺
                    </td>
                </tr>"
            ));

            var orderSummary =
            $@"
<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;background:#f3f4f6;font-family:Arial,sans-serif;color:#111827;'>

    <div style='max-width:680px;margin:0 auto;padding:28px;'>

        <div style='background:linear-gradient(135deg,#0f172a,#1e293b);padding:28px;border-radius:22px 22px 0 0;color:white;'>
            <div style='font-size:13px;font-weight:800;letter-spacing:.08em;text-transform:uppercase;color:#bfdbfe;'>
                Food4Everyone Catering Platform
            </div>

            <h1 style='margin:12px 0 8px 0;font-size:30px;line-height:1.15;'>
                Catering Order Confirmation
            </h1>

            <p style='margin:0;color:#cbd5e1;font-size:15px;line-height:1.6;'>
                Your catering order has been received successfully.
            </p>
        </div>

        <div style='background:white;padding:26px;border-left:1px solid #e5e7eb;border-right:1px solid #e5e7eb;'>

            <div style='display:flex;gap:12px;flex-wrap:wrap;margin-bottom:20px;'>
                <div style='background:#eff6ff;border:1px solid #bfdbfe;border-radius:16px;padding:14px;min-width:180px;'>
                    <div style='font-size:12px;color:#64748b;font-weight:800;text-transform:uppercase;'>Order Number</div>
                    <div style='font-size:22px;color:#0f172a;font-weight:900;margin-top:4px;'>#{order.Id}</div>
                </div>

                <div style='background:#fef3c7;border:1px solid #fde68a;border-radius:16px;padding:14px;min-width:180px;'>
                    <div style='font-size:12px;color:#92400e;font-weight:800;text-transform:uppercase;'>Status</div>
                    <div style='font-size:18px;color:#92400e;font-weight:900;margin-top:4px;'>{order.Status}</div>
                </div>

                <div style='background:#dcfce7;border:1px solid #bbf7d0;border-radius:16px;padding:14px;min-width:180px;'>
                    <div style='font-size:12px;color:#166534;font-weight:800;text-transform:uppercase;'>Total Amount</div>
                    <div style='font-size:22px;color:#166534;font-weight:900;margin-top:4px;'>{order.TotalPrice:0.00} ₺</div>
                </div>
            </div>

            <div style='background:#f8fafc;border:1px solid #e2e8f0;border-radius:18px;padding:18px;margin-bottom:22px;'>
                <h2 style='margin:0 0 12px 0;color:#0f172a;font-size:18px;'>Customer Information</h2>

                <p style='margin:6px 0;color:#334155;'>
                    <strong>Email:</strong> {user?.Email}
                </p>

                <p style='margin:6px 0;color:#334155;'>
                    <strong>Delivery Address:</strong> {user?.Address}
                </p>

                <p style='margin:6px 0;color:#334155;'>
                    <strong>Order Date:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}
                </p>
            </div>

            <h2 style='margin:0 0 14px 0;color:#0f172a;font-size:20px;'>
                Ordered Catering Packages
            </h2>

            <table style='width:100%;border-collapse:collapse;border:1px solid #e5e7eb;border-radius:16px;overflow:hidden;'>
                <thead>
                    <tr style='background:#0f172a;color:white;'>
                        <th style='padding:14px;text-align:left;font-size:13px;'>Package Details</th>
                        <th style='padding:14px;text-align:right;font-size:13px;'>Line Total</th>
                    </tr>
                </thead>

                <tbody>
                    {packageRows}
                </tbody>
            </table>

            <div style='margin-top:24px;padding:18px;background:#ecfdf5;border:1px solid #bbf7d0;border-radius:18px;'>
                <strong style='color:#166534;font-size:16px;'>Next Step</strong>
                <p style='margin:8px 0 0 0;color:#166534;line-height:1.6;'>
                    The catering company will review your order and update the order status.
                    You can follow the status from your Food4Everyone account.
                </p>
            </div>

        </div>

        <div style='background:#0f172a;color:#cbd5e1;padding:18px 26px;border-radius:0 0 22px 22px;text-align:center;font-size:13px;'>
            Food4Everyone © 2026 — Smart Catering Platform
        </div>

    </div>

</body>
</html>";

            if (user?.Email != null)
{
    _emailService.SendOrderEmail(
        user.Email,
        $"Food4Everyone Catering Order #{order.Id}",
        orderSummary);
}

var orderedMenuItemIds = cart
    .Select(c => c.MenuItemId)
    .ToList();

var orderedMenuItems = await _context.MenuItems
    .Where(m => orderedMenuItemIds.Contains(m.Id))
    .ToListAsync();

var caretakerIds = orderedMenuItems
    .Where(m => m.CaretakerId != null)
    .Select(m => m.CaretakerId!)
    .Distinct()
    .ToList();

foreach (var caretakerId in caretakerIds)
{
    var caretaker =
        await _userManager.FindByIdAsync(caretakerId);

    if (caretaker?.Email == null)
    {
        continue;
    }

    var caretakerMenuItemIds = orderedMenuItems
        .Where(m => m.CaretakerId == caretakerId)
        .Select(m => m.Id)
        .ToList();

    var caretakerCartItems = cart
        .Where(c => caretakerMenuItemIds.Contains(c.MenuItemId))
        .ToList();

    var caretakerPackageRows = string.Join("", caretakerCartItems.Select(x =>
        $@"
        <tr>
            <td style='padding:14px;border-bottom:1px solid #e5e7eb;'>
                <strong style='color:#111827;font-size:15px;'>{x.MenuItemName}</strong>
                <div style='color:#6b7280;font-size:13px;margin-top:4px;'>
                    Guest Count: {x.PersonCount}
                </div>
                <div style='color:#6b7280;font-size:13px;margin-top:4px;'>
                    Features: {(x.SelectedOptionNames.Any() ? string.Join(", ", x.SelectedOptionNames) : "No additional features")}
                </div>
            </td>
            <td style='padding:14px;border-bottom:1px solid #e5e7eb;text-align:right;font-weight:800;color:#166534;white-space:nowrap;'>
                {x.LineTotal:0.00} ₺
            </td>
        </tr>"
    ));

    var caretakerTotal = caretakerCartItems.Sum(x => x.LineTotal);

    var caretakerOrderSummary =
    $@"
<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;background:#f3f4f6;font-family:Arial,sans-serif;color:#111827;'>

    <div style='max-width:680px;margin:0 auto;padding:28px;'>

        <div style='background:linear-gradient(135deg,#166534,#15803d);padding:28px;border-radius:22px 22px 0 0;color:white;'>
            <div style='font-size:13px;font-weight:800;letter-spacing:.08em;text-transform:uppercase;color:#dcfce7;'>
                Food4Everyone Catering Platform
            </div>

            <h1 style='margin:12px 0 8px 0;font-size:30px;line-height:1.15;'>
                New Catering Order Received
            </h1>

            <p style='margin:0;color:#dcfce7;font-size:15px;line-height:1.6;'>
                A customer has placed an order for your catering package.
            </p>
        </div>

        <div style='background:white;padding:26px;border-left:1px solid #e5e7eb;border-right:1px solid #e5e7eb;'>

            <div style='display:flex;gap:12px;flex-wrap:wrap;margin-bottom:20px;'>
                <div style='background:#eff6ff;border:1px solid #bfdbfe;border-radius:16px;padding:14px;min-width:180px;'>
                    <div style='font-size:12px;color:#64748b;font-weight:800;text-transform:uppercase;'>Order Number</div>
                    <div style='font-size:22px;color:#0f172a;font-weight:900;margin-top:4px;'>#{order.Id}</div>
                </div>

                <div style='background:#fef3c7;border:1px solid #fde68a;border-radius:16px;padding:14px;min-width:180px;'>
                    <div style='font-size:12px;color:#92400e;font-weight:800;text-transform:uppercase;'>Status</div>
                    <div style='font-size:18px;color:#92400e;font-weight:900;margin-top:4px;'>{order.Status}</div>
                </div>

                <div style='background:#dcfce7;border:1px solid #bbf7d0;border-radius:16px;padding:14px;min-width:180px;'>
                    <div style='font-size:12px;color:#166534;font-weight:800;text-transform:uppercase;'>Your Order Amount</div>
                    <div style='font-size:22px;color:#166534;font-weight:900;margin-top:4px;'>{caretakerTotal:0.00} ₺</div>
                </div>
            </div>

            <div style='background:#f8fafc;border:1px solid #e2e8f0;border-radius:18px;padding:18px;margin-bottom:22px;'>
                <h2 style='margin:0 0 12px 0;color:#0f172a;font-size:18px;'>Customer Information</h2>

                <p style='margin:6px 0;color:#334155;'>
                    <strong>Customer Email:</strong> {user?.Email}
                </p>

                <p style='margin:6px 0;color:#334155;'>
                    <strong>Delivery Address:</strong> {user?.Address}
                </p>

                <p style='margin:6px 0;color:#334155;'>
                    <strong>Order Date:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}
                </p>
            </div>

            <h2 style='margin:0 0 14px 0;color:#0f172a;font-size:20px;'>
                Ordered Packages From Your Company
            </h2>

            <table style='width:100%;border-collapse:collapse;border:1px solid #e5e7eb;border-radius:16px;overflow:hidden;'>
                <thead>
                    <tr style='background:#166534;color:white;'>
                        <th style='padding:14px;text-align:left;font-size:13px;'>Package Details</th>
                        <th style='padding:14px;text-align:right;font-size:13px;'>Line Total</th>
                    </tr>
                </thead>

                <tbody>
                    {caretakerPackageRows}
                </tbody>
            </table>

            <div style='margin-top:24px;padding:18px;background:#eff6ff;border:1px solid #bfdbfe;border-radius:18px;'>
                <strong style='color:#1d4ed8;font-size:16px;'>Action Required</strong>
                <p style='margin:8px 0 0 0;color:#1d4ed8;line-height:1.6;'>
                    Please log in to your Food4Everyone caretaker dashboard and update the order status.
                </p>
            </div>

        </div>

        <div style='background:#0f172a;color:#cbd5e1;padding:18px 26px;border-radius:0 0 22px 22px;text-align:center;font-size:13px;'>
            Food4Everyone © 2026 — Smart Catering Platform
        </div>

    </div>

</body>
</html>";

    _emailService.SendOrderEmail(
        caretaker.Email,
        $"New Catering Order #{order.Id}",
        caretakerOrderSummary);
}

            await _logService.AddLogAsync(
                "Catering Emails Sent",
                $"Email notifications generated for catering order #{order.Id}.",
                userId);

            HttpContext.Session.Remove(CartSessionKey);

            return RedirectToAction(nameof(Success),
                new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;

            return View();
        }
    }
}