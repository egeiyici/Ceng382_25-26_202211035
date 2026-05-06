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

        public async Task<IActionResult> Index()
        {
            var logs = await _context.LogEntries
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(logs);
        }
    }
}