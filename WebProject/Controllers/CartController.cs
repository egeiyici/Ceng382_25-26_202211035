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
            var cart =
                HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey)
                ?? new List<CartItem>();

            return View(cart);
        }

        public async Task<IActionResult> Details(int menuItemId)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.MenuOptions)
                .Include(m => m.Caretaker)
                .Include(m => m.Ratings)
                .FirstOrDefaultAsync(m => m.Id == menuItemId);

            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            int menuItemId,
            int personCount,
            List<int>? selectedOptionIds)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.MenuOptions)
                .FirstOrDefaultAsync(m => m.Id == menuItemId);

            if (menuItem == null)
            {
                return NotFound();
            }

            if (personCount < menuItem.MinimumPeople)
            {
                personCount = menuItem.MinimumPeople;
            }

            selectedOptionIds ??= new List<int>();

            var selectedOptions = menuItem.MenuOptions
                .Where(o => selectedOptionIds.Contains(o.Id))
                .ToList();

            var customizationTotal =
                selectedOptions.Sum(o => o.ExtraPrice);

            var cart =
                HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey)
                ?? new List<CartItem>();

            var orderedOptionIds = selectedOptionIds
                .OrderBy(x => x)
                .ToList();

            var existingItem = cart.FirstOrDefault(c =>
                c.MenuItemId == menuItem.Id &&
                c.PersonCount == personCount &&
                c.SelectedOptionIds.OrderBy(x => x).SequenceEqual(orderedOptionIds));

            if (existingItem != null)
            {
                existingItem.SelectedOptionIds = orderedOptionIds;
                existingItem.SelectedOptionNames = selectedOptions
                    .Select(o => o.OptionName)
                    .ToList();
                existingItem.CustomizationTotal = customizationTotal;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MenuItemId = menuItem.Id,
                    MenuItemName = menuItem.Name,
                    BasePrice = menuItem.BasePrice,
                    MinimumPeople = menuItem.MinimumPeople,
                    PricePerExtraPerson = menuItem.PricePerExtraPerson,
                    PersonCount = personCount,
                    Quantity = 1,
                    SelectedOptionIds = orderedOptionIds,
                    SelectedOptionNames = selectedOptions
                        .Select(o => o.OptionName)
                        .ToList(),
                    CustomizationTotal = customizationTotal
                });
            }

            HttpContext.Session.SetObject(CartSessionKey, cart);

            await _logService.AddLogAsync(
                "Catering Package Added To Cart",
                $"{menuItem.Name} catering package added for {personCount} guests.",
                _userManager.GetUserId(User));

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int menuItemId)
        {
            var cart =
                HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey)
                ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);

            if (item != null)
            {
                cart.Remove(item);

                await _logService.AddLogAsync(
                    "Catering Package Removed",
                    $"{item.MenuItemName} was removed from cart.",
                    _userManager.GetUserId(User));
            }

            HttpContext.Session.SetObject(CartSessionKey, cart);

            return RedirectToAction(nameof(Index));
        }
    }
}