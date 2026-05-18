using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;

namespace WebProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            int pageSize = 10;

            var query = _context.LogEntries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(l =>
                    l.EventType.Contains(searchTerm) ||
                    (l.Description != null && l.Description.Contains(searchTerm)) ||
                    (l.UserId != null && l.UserId.Contains(searchTerm)));
            }

            var totalItems = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(logs);
        }
    }
}