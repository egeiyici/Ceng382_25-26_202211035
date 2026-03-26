using Lab4.Data;
using Lab4.Models.Identity;
using Lab4.Models.Media;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Ceng382_25_26_202211035.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public List<ImageModel> Images { get; set; } = new List<ImageModel>();
        public List<Student> Students { get; set; } = new List<Student>();

        [BindProperty]
        public List<IFormFile> GalleryFiles { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _context.Users.ToListAsync();
            Images = await _context.Images.ToListAsync();

            Students = new List<Student>
            {
                new Student { Id = 1, Name = "Ayşe Kaya", Midterm = 78, Final = 85 },
                new Student { Id = 2, Name = "Mehmet Demir", Midterm = 65, Final = 72 },
                new Student { Id = 3, Name = "Elif Yılmaz", Midterm = 90, Final = 93 },
                new Student { Id = 4, Name = "Ali Can", Midterm = 50, Final = 60 }
            };
        }

        public async Task<IActionResult> OnPostUploadGalleryAsync()
        {
            if (GalleryFiles != null && GalleryFiles.Count > 0)
            {
                foreach (var file in GalleryFiles)
                {
                    if (file.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);

                        var image = new ImageModel
                        {
                            FileName = file.FileName,
                            ContentType = file.ContentType,
                            Size = file.Length,
                            ImageData = memoryStream.ToArray()
                        };

                        _context.Images.Add(image);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double Midterm { get; set; }
        public double Final { get; set; }
        public string LetterGrade { get; set; } = "";
    }
}