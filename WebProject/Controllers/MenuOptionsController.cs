using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using WebProject.Services;

namespace WebProject.Controllers
{
    [Authorize(Roles = "Caretaker")]
    public class MenuOptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _logService;

        public MenuOptionsController(
            ApplicationDbContext context,
            LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Create(int menuItemId)
        {
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);

            if (menuItem == null)
            {
                return NotFound();
            }

            ViewBag.MenuItemId = menuItem.Id;
            ViewBag.MenuItemName = menuItem.Name;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuOption menuOption)
        {
            if (ModelState.IsValid)
            {
                _context.MenuOptions.Add(menuOption);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync(
                    "Package Option Created",
                    $"Package option '{menuOption.OptionName}' was added to catering package #{menuOption.MenuItemId}.",
                    null);

                return RedirectToAction("Index", "MenuItems");
            }

            return View(menuOption);
        }
    }
}