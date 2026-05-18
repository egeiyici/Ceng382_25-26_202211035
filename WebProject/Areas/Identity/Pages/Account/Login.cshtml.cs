#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebProject.Models;
using WebProject.Services;

namespace WebProject.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly LogService _logService;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            EmailService emailService,
            LogService logService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _logService = logService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                await _logService.AddLogAsync("Failed Login", $"Failed login attempt for unknown email {Input.Email}.", null);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);

            if (!passwordValid)
            {
                await _logService.AddLogAsync("Failed Login", $"Failed login attempt for {Input.Email}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            if (user.TwoFactorEnabled == true)
            {
                var code = Random.Shared.Next(100000, 999999).ToString();

                HttpContext.Session.SetString("TwoFactorCode", code);
                HttpContext.Session.SetString("TwoFactorUserId", user.Id);
                HttpContext.Session.SetString("RememberMe", Input.RememberMe.ToString());
                HttpContext.Session.SetString("ReturnUrl", returnUrl);

                _emailService.SendOrderEmail(
                    user.Email!,
                    "Food4Everyone Two-Factor Login Code",
                    $"Your two-factor authentication code is: {code}");

                await _logService.AddLogAsync(
                    "2FA Code Sent",
                    $"Two-factor authentication code was generated for {user.Email}.",
                    user.Id);

                return LocalRedirect("/Identity/Account/VerifyTwoFactor");
            }

            await _signInManager.SignInAsync(user, Input.RememberMe);

            await _logService.AddLogAsync(
                "Successful Login",
                $"{user.Email} logged in successfully.",
                user.Id);

            return await RedirectByRoleAsync(user, returnUrl);
        }

        private async Task<IActionResult> RedirectByRoleAsync(ApplicationUser user, string returnUrl)
        {
            if (await _userManager.IsInRoleAsync(user, "Caretaker"))
            {
                return LocalRedirect("/Dashboard");
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return LocalRedirect("/Dashboard");
            }

            return LocalRedirect("/");
        }
    }
}