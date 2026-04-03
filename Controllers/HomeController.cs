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

    public IActionResult Index()
    {
        return View(FakeShipperStore.Data);
    }

    public IActionResult Edit(int id)
    {
        var item = FakeShipperStore.Data.FirstOrDefault(x => x.ShipperId == id);

        if (item == null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost]
    public IActionResult Edit(int id, ShipperDisplayViewModel model)
    {
        if (id != model.ShipperId)
        {
            return BadRequest();
        }

        var item = FakeShipperStore.Data.FirstOrDefault(x => x.ShipperId == id);

        if (item == null)
        {
            return NotFound();
        }

        item.Id = model.Id;
        item.Email = model.Email;
        item.Website = model.Website;
        item.Phone = model.Phone;
        item.Address = model.Address;
        item.City = model.City;
        item.Country = model.Country;
        item.PostCode = model.PostCode;
        item.IsVisible = model.IsVisible;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> HardDelete(int id)
    {
        var shipper = await _db.Shippers.FindAsync(id);

        if (shipper != null)
        {
            var orders = await _db.Orders
                .Where(o => o.ShipVia == id)
                .ToListAsync();

            var orderIds = orders.Select(o => o.OrderId).ToList();

            if (orderIds.Any())
            {
                var orderDetails = await _db.OrderDetails
                    .Where(od => orderIds.Contains(od.OrderId))
                    .ToListAsync();

                _db.OrderDetails.RemoveRange(orderDetails);
            }

            _db.Orders.RemoveRange(orders);
            _db.Shippers.Remove(shipper);

            await _db.SaveChangesAsync();
        }

        var fakeItem = FakeShipperStore.Data.FirstOrDefault(x => x.ShipperId == id);
        if (fakeItem != null)
        {
            FakeShipperStore.Data.Remove(fakeItem);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult SoftDelete(int id)
    {
        var item = FakeShipperStore.Data.FirstOrDefault(x => x.ShipperId == id);

        if (item != null)
        {
            item.IsVisible = false;
        }

        return RedirectToAction(nameof(Index));
    }
}