#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebProject.Models;
using WebProject.Services;

namespace WebProject.Areas.Identity.Pages.Account
{
    public class VerifyTwoFactorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly LogService _logService;

        public VerifyTwoFactorModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            LogService logService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logService = logService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Code { get; set; }
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
var userId = HttpContext.Session.GetString("TwoFactorUserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("./Login");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToPage("./Login");
            }

           var sessionCode = HttpContext.Session.GetString("TwoFactorCode");

var valid = sessionCode == Input.Code;
            if (!valid)
            {
                ModelState.AddModelError(string.Empty, "Invalid verification code.");

                await _logService.AddLogAsync(
                    "Failed 2FA",
                    $"Invalid two-factor code attempt for {user.Email}.",
                    user.Id);

                TempData["TwoFactorUserId"] = user.Id;

                return Page();
            }

            bool rememberMe = false;

            var rememberMeValue = HttpContext.Session.GetString("RememberMe");

if (!string.IsNullOrEmpty(rememberMeValue))
{
    bool.TryParse(rememberMeValue, out rememberMe);
}

            await _signInManager.SignInAsync(user, rememberMe);

            await _logService.AddLogAsync(
                "Successful 2FA Login",
                $"{user.Email} completed two-factor authentication.",
                user.Id);

            if (await _userManager.IsInRoleAsync(user, "Caretaker"))
            {
                return Redirect("/Dashboard");
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Redirect("/Dashboard");
            }

            return Redirect("/");
        }
    }
}