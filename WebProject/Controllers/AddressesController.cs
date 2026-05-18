using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebProject.Models;
using WebProject.Services;

namespace WebProject.Controllers
{
    [Authorize(Roles = "User")]
    public class AddressesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LogService _logService;

        public AddressesController(
            UserManager<ApplicationUser> userManager,
            LogService logService)
        {
            _userManager = userManager;
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAddress(
            string address,
            string latitude,
            string longitude,
            string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var parsedLatitude =
                double.Parse(latitude, CultureInfo.InvariantCulture);

            var parsedLongitude =
                double.Parse(longitude, CultureInfo.InvariantCulture);

            user.Address = address;
            user.Latitude = parsedLatitude;
            user.Longitude = parsedLongitude;

            await _userManager.UpdateAsync(user);

            await _logService.AddLogAsync(
                "User Address Updated",
                $"{user.Email} updated delivery address.",
                user.Id);

            TempData["SuccessMessage"] =
                "Delivery address saved successfully.";

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}