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
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LogService _logService;

        public MenuItemsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            LogService logService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
        }

      public async Task<IActionResult> Index(
    string? searchTerm,
    string? caretakerId,
    int page = 1)
{
    int pageSize = 6;

    var query = _context.MenuItems
        .Include(m => m.Caretaker)
        .Include(m => m.Ratings)
        .AsQueryable();

    if (User.IsInRole("Caretaker"))
    {
        var currentCaretakerId = _userManager.GetUserId(User);
        query = query.Where(m => m.CaretakerId == currentCaretakerId);
    }
    else if (!string.IsNullOrWhiteSpace(caretakerId))
    {
        query = query.Where(m => m.CaretakerId == caretakerId);
    }

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        query = query.Where(m =>
            m.Name.Contains(searchTerm) ||
            (m.Description != null &&
             m.Description.Contains(searchTerm)));
    }

    var totalItems = await query.CountAsync();

    var menuItems = await query
        .OrderBy(m => m.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    ViewBag.SearchTerm = searchTerm;
    ViewBag.CurrentPage = page;
    ViewBag.TotalPages =
        (int)Math.Ceiling(totalItems / (double)pageSize);

    return View(menuItems);
}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Caretaker)
                .Include(m => m.Ratings)
                .Include(m => m.MenuOptions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return RedirectToAction("Details", "Cart", new { menuItemId = menuItem.Id });
        }

        [Authorize(Roles = "Caretaker")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Caretaker")]
        public async Task<IActionResult> Create(MenuItem menuItem, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                menuItem.CaretakerId = _userManager.GetUserId(User);

                if (imageFile != null && imageFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await imageFile.CopyToAsync(memoryStream);

                    menuItem.ImageData = memoryStream.ToArray();
                    menuItem.ImageContentType = imageFile.ContentType;
                }

                _context.Add(menuItem);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync(
                    "Catering Package Created",
                    $"{menuItem.Name} was created by a catering company.",
                    _userManager.GetUserId(User));

                return RedirectToAction(nameof(Index));
            }

            return View(menuItem);
        }

        [Authorize(Roles = "Caretaker")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems.FindAsync(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            if (menuItem.CaretakerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View(menuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Caretaker")]
       public async Task<IActionResult> Edit(
    int id,
    MenuItem menuItem,
    IFormFile? imageFile,
    bool removeImage = false)
        {
            if (id != menuItem.Id)
            {
                return NotFound();
            }

            var existingItem = await _context.MenuItems.FindAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            if (existingItem.CaretakerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                existingItem.Name = menuItem.Name;
                existingItem.Price = menuItem.Price;
                existingItem.Description = menuItem.Description;

               if (removeImage)
{
    existingItem.ImageData = null;
    existingItem.ImageContentType = null;
}
else if (imageFile != null && imageFile.Length > 0)
{
    using var memoryStream = new MemoryStream();
    await imageFile.CopyToAsync(memoryStream);

    existingItem.ImageData = memoryStream.ToArray();
    existingItem.ImageContentType = imageFile.ContentType;
}
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync(
                    "Catering Package Edited",
                    $"{existingItem.Name} was updated by a catering company.",
                    _userManager.GetUserId(User));

                return RedirectToAction(nameof(Index));
            }

            return View(menuItem);
        }

        [Authorize(Roles = "Caretaker")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Caretaker)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            if (menuItem.CaretakerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View(menuItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Caretaker")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);

            if (menuItem != null)
            {
                if (menuItem.CaretakerId != _userManager.GetUserId(User))
                {
                    return Forbid();
                }

                var deletedName = menuItem.Name;

                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync(
                    "Catering Package Deleted",
                    $"{deletedName} was deleted by a catering company.",
                    _userManager.GetUserId(User));
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Image(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);

            if (menuItem == null ||
                menuItem.ImageData == null ||
                menuItem.ImageContentType == null)
            {
                return NotFound();
            }

            return File(menuItem.ImageData, menuItem.ImageContentType);
        }
    }
}