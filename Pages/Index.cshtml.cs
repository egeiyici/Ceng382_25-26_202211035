using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ceng382_25_26_202211035.Pages
{
    public class IndexModel : PageModel
    {
        public List<Student> Students { get; set; } = new List<Student>();

        public void OnGet()
        {
            Students = new List<Student>
            {
                new Student { Id = 1, Name = "Ayşe ", Midterm = 78, Final = 85 },
                new Student { Id = 2, Name = "Mehmet ", Midterm = 65, Final = 72 },
                new Student { Id = 3, Name = "Elif Zeynep", Midterm = 90, Final = 93 },
                
            };
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