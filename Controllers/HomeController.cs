using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ceng382_25_26_202211035.Models;
namespace Ceng382_25_26_202211035.Controllers;

public class HomeController : Controller
{
    private readonly NorthwindContext _db;

    public HomeController(NorthwindContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
{
    var shippers = await _db.Shippers.ToListAsync();
    return View(shippers);
}
    public async Task<IActionResult> ProductDetail(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest("You must pass a product ID in the route, for example, /Home/ProductDetail/21");
        }

        Product? model = await _db.Products
            .Include(p => p.Category)
            .SingleOrDefaultAsync(p => p.ProductId == id);

        if (model is null)
        {
            return NotFound($"ProductId {id} not found.");
        }

        return View(model);
    }

    public IActionResult ModelBinding()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ModelBinding(Thing thing)
    {
        HomeModelBindingViewModel model = new(
            Thing: thing,
            HasErrors: !ModelState.IsValid,
            ValidationErrors: ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
        );

        return View(model);
    }
}