using Microsoft.AspNetCore.Identity;

namespace WebProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}