using Microsoft.AspNetCore.Identity;

namespace Lab4.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Address { get; set; }

    public byte[]? ProfilePhoto { get; set; }
    public string? ContentType { get; set; }
}