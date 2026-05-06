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
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _logService;
        private readonly UserManager<ApplicationUser> _userManager;

        private const string CartSessionKey = "Cart";

        public CartController(
            ApplicationDbContext context,
            LogService logService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logService = logService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int menuItemId, List<int>? selectedOptionIds)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.MenuOptions)
                .FirstOrDefaultAsync(m => m.Id == menuItemId);

            if (menuItem == null)
            {
                return NotFound();
            }

            selectedOptionIds ??= new List<int>();

            var selectedOptions = menuItem.MenuOptions
                .Where(o => selectedOptionIds.Contains(o.Id))
                .ToList();

            var customizationTotal = selectedOptions.Sum(o => o.ExtraPrice);

            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(c =>
                c.MenuItemId == menuItem.Id &&
                c.SelectedOptionIds.SequenceEqual(selectedOptionIds));

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MenuItemId = menuItem.Id,
                    MenuItemName = menuItem.Name,
                    UnitPrice = menuItem.Price,
                    Quantity = 1,
                    SelectedOptionIds = selectedOptionIds,
                    SelectedOptionNames = selectedOptions.Select(o => o.OptionName).ToList(),
                    CustomizationTotal = customizationTotal
                });
            }

            HttpContext.Session.SetObject(CartSessionKey, cart);

            var userId = _userManager.GetUserId(User);

            await _logService.AddLogAsync(
                "Cart Item Added",
                $"{menuItem.Name} was added to cart.",
                userId);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int menuItemId, int quantity)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }

            HttpContext.Session.SetObject(CartSessionKey, cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int menuItemId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);

            if (item != null)
            {
                cart.Remove(item);
            }

            HttpContext.Session.SetObject(CartSessionKey, cart);
            return RedirectToAction(nameof(Index));
        }
    }
}