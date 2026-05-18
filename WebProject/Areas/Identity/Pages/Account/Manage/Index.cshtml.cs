#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebProject.Models;

namespace WebProject.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        public bool HasProfileImage { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Full Name / Company Name")]
            public string FullName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public IFormFile ProfileImage { get; set; }

            public bool RemoveProfileImage { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            HasProfileImage = user.ProfileImageData != null;

            Input = new InputModel
            {
                FullName = user.FullName,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetProfileImageAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null ||
                user.ProfileImageData == null ||
                user.ProfileImageContentType == null)
            {
                return NotFound();
            }

            return File(user.ProfileImageData, user.ProfileImageContentType);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound(
                    $"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound(
                    $"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            user.FullName = Input.FullName;

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult =
                    await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);

                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage =
                        "Unexpected error when trying to set phone number.";

                    return RedirectToPage();
                }
            }

            if (Input.RemoveProfileImage)
            {
                user.ProfileImageData = null;
                user.ProfileImageContentType = null;
            }
            else if (Input.ProfileImage != null &&
                     Input.ProfileImage.Length > 0)
            {
                using var memoryStream = new MemoryStream();

                await Input.ProfileImage.CopyToAsync(memoryStream);

                user.ProfileImageData = memoryStream.ToArray();
                user.ProfileImageContentType = Input.ProfileImage.ContentType;
            }

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Your profile has been updated.";

            return RedirectToPage();
        }
    }
}