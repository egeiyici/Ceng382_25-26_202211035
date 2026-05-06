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

        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

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
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

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
                Status = "Completed",
                TotalPrice = cart.Sum(x => x.LineTotal)
            };

            foreach (var cartItem in cart)
            {
                var orderItem = new OrderItem
                {
                    MenuItemId = cartItem.MenuItemId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
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
                "Payment Completed",
                $"Order #{order.Id} was created with total price {order.TotalPrice}.",
                userId);

            var orderSummary =
                $"Order ID: {order.Id}\n" +
                $"Customer: {user?.Email}\n" +
                $"Total Price: {order.TotalPrice} ₺\n" +
                $"Order Date: {order.CreatedAt}\n" +
                $"Status: {order.Status}\n\n" +
                "Items:\n" +
                string.Join("\n", cart.Select(x =>
                    $"- {x.MenuItemName} x{x.Quantity} | Options: {(x.SelectedOptionNames.Any() ? string.Join(", ", x.SelectedOptionNames) : "No customization")} | Line Total: {x.LineTotal} ₺"));

            if (user?.Email != null)
            {
                _emailService.SendOrderEmail(
                    user.Email,
                    "Your Food4Everyone Order",
                    orderSummary);
            }

            var caretakerIds = await _context.MenuItems
                .Where(m => cart.Select(c => c.MenuItemId).Contains(m.Id))
                .Select(m => m.CaretakerId)
                .Where(id => id != null)
                .Distinct()
                .ToListAsync();

            foreach (var caretakerId in caretakerIds)
            {
                var caretaker = await _userManager.FindByIdAsync(caretakerId!);

                if (caretaker?.Email != null)
                {
                    _emailService.SendOrderEmail(
                        caretaker.Email,
                        "New Incoming Food4Everyone Order",
                        orderSummary);
                }
            }

            await _logService.AddLogAsync(
                "Email Sent",
                $"Order #{order.Id} email notifications were generated.",
                userId);

            HttpContext.Session.Remove(CartSessionKey);

            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }
}