using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProject.Models;

namespace WebProject.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var context =
                serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "Caretaker", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await SeedAdminAsync(userManager);
            await SeedCaretakersAsync(userManager, context);
        }
        private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
{
    var adminEmail = "admin@gmail.com";

    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

    if (existingAdmin != null)
    {
        return;
    }

    var admin = new ApplicationUser
    {
        UserName = adminEmail,
        Email = adminEmail,
        FullName = "System Administrator",
        EmailConfirmed = true
    };

    await userManager.CreateAsync(admin, "Test123!");
    await userManager.AddToRoleAsync(admin, "Admin");
}
        
        

        private static async Task SeedCaretakersAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            if (await context.MenuItems.AnyAsync())
            {
                return;
            }

            var caretakerData = new List<(string District, string CompanyName, double Lat, double Lng, string Package1, string Package2, decimal Base1, decimal Extra1, int Min1, decimal Base2, decimal Extra2, int Min2)>
            {
                ("Cankaya", "Prime Table Catering", 39.9179, 32.8627, "Corporate Lunch Meeting Package", "Diplomatic Reception Catering", 3600, 190, 10, 12800, 330, 40),
                ("Kecioren", "Family Feast Ankara", 39.9757, 32.8663, "Family Celebration Package", "Large Community Dinner Package", 3200, 170, 12, 8700, 260, 30),
                ("Yenimahalle", "Metro Catering Co.", 39.9674, 32.8090, "Office Breakfast Package", "Factory Staff Lunch Package", 2800, 150, 10, 9400, 210, 45),
                ("Mamak", "Budget Banquet Services", 39.9308, 32.9306, "Budget Event Catering", "School Ceremony Package", 2400, 130, 10, 6200, 185, 25),
                ("Etimesgut", "Blue Line Catering", 39.9439, 32.6498, "Military Style Group Meal Package", "Outdoor Picnic Catering", 4100, 175, 15, 7600, 230, 35),
                ("Sincan", "Industrial Taste Catering", 39.9667, 32.5833, "Industrial Lunch Package", "Wedding Starter Catering", 3900, 165, 20, 9800, 270, 40),
                ("Golbasi", "Lake Garden Events", 39.7904, 32.8086, "Lake View Wedding Package", "Garden Party Catering", 7200, 260, 25, 14500, 390, 60),
                ("Pursaklar", "Skyline Event Catering", 40.0390, 32.9014, "Graduation Catering Package", "Engagement Ceremony Package", 4500, 190, 20, 9300, 285, 35),
                ("Altindag", "Heritage Ankara Catering", 39.9496, 32.8540, "Traditional Ankara Dinner Package", "Museum Event Catering", 5200, 210, 18, 11200, 320, 45),
                ("Polatli", "Harvest Catering House", 39.5771, 32.1413, "Rural Wedding Catering", "Harvest Festival Package", 6800, 230, 30, 11800, 300, 60),
                ("Beypazari", "Bey Gourmet Catering", 40.1672, 31.9211, "Traditional Beypazari Cuisine Package", "Local Dessert Catering", 5000, 200, 15, 8200, 250, 30),
                ("Kahramankazan", "Kazan Corporate Meals", 40.1650, 32.6397, "Corporate Factory Meal Package", "Business Seminar Catering", 4600, 175, 25, 9900, 245, 50),
                ("Cubuk", "Regional Taste Catering", 40.2386, 33.0322, "Regional Dinner Package", "Town Festival Catering", 4300, 185, 18, 8900, 260, 40),
                ("Akyurt", "Aero Catering Solutions", 40.1308, 33.0872, "Airport Business Catering", "Logistics Company Lunch Package", 5500, 220, 20, 10500, 275, 50),
                ("Elmadag", "Summit Catering", 39.9200, 33.2300, "Mountain Event Catering", "Winter Organization Package", 4800, 195, 18, 9700, 285, 35),
                ("Haymana", "Thermal Taste Events", 39.4321, 32.4973, "Thermal Hotel Catering", "Family Gathering Package", 5300, 210, 20, 8600, 245, 35),
                ("Bala", "Countryside Banquet", 39.5533, 33.1235, "Countryside Wedding Package", "Village Celebration Catering", 6100, 225, 30, 9500, 270, 45),
                ("Nallihan", "Nature Table Catering", 40.1856, 31.3519, "Nature Camp Catering", "Traditional Guest Meal Package", 4200, 180, 15, 7900, 235, 30),
                ("Kalecik", "Vineyard Catering House", 40.0972, 33.4083, "Vineyard Event Catering", "Wine House Dinner Package", 6500, 255, 25, 12400, 360, 50),
                ("Ayas", "Thermal Banquet Ayas", 40.0195, 32.3321, "Thermal Resort Package", "Classic Turkish Dinner Package", 5000, 205, 20, 9100, 265, 40),
                ("Gudul", "Local Culture Catering", 40.2104, 32.2450, "Small Town Wedding Package", "Local Culture Event Catering", 5700, 215, 25, 8700, 250, 35),
                ("Evren", "Lake Side Catering", 39.0245, 33.8061, "Lake Side Organization Package", "Municipality Event Catering", 4600, 190, 20, 8400, 245, 40),
                ("Camlidere", "Forest Event Catering", 40.4897, 32.4747, "Nature Wedding Catering", "Camp Group Meal Package", 6200, 235, 25, 8800, 255, 40),
                ("Kizilcahamam", "Thermal Conference Catering", 40.4703, 32.6506, "Thermal Conference Catering", "Mountain Picnic Package", 5900, 230, 25, 9300, 275, 45),
                ("Sereflikochisar", "Salt Lake Catering", 38.9392, 33.5390, "Salt Lake Event Package", "Regional Wedding Catering", 5400, 210, 20, 9700, 280, 45)
            };

            foreach (var item in caretakerData)
            {
var index = caretakerData.IndexOf(item) + 1;

var email = $"caretaker{index}@gmail.com";
                var existingUser = await userManager.FindByEmailAsync(email);

                ApplicationUser caretaker;

                if (existingUser == null)
                {
                    caretaker = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = item.CompanyName,
                        Address = $"{item.District}, Ankara",
                        Latitude = item.Lat,
                        Longitude = item.Lng
                    };

                    await userManager.CreateAsync(caretaker, "Selam123.");
                    await userManager.AddToRoleAsync(caretaker, "Caretaker");
                }
                else
                {
                    caretaker = existingUser;
                }

                var package1 = new MenuItem
                {
                    Name = item.Package1,
                    Description = $"A professional catering package prepared by {item.CompanyName} for events and organizations in {item.District}. Includes main dishes, side options, and flexible service features.",
                    BasePrice = item.Base1,
                    PricePerExtraPerson = item.Extra1,
                    MinimumPeople = item.Min1,
                    CaretakerId = caretaker.Id
                };

                var package2 = new MenuItem
                {
                    Name = item.Package2,
                    Description = $"A larger catering solution prepared by {item.CompanyName} for special events in {item.District}. Suitable for ceremonies, company gatherings, and group celebrations.",
                    BasePrice = item.Base2,
                    PricePerExtraPerson = item.Extra2,
                    MinimumPeople = item.Min2,
                    CaretakerId = caretaker.Id
                };

                context.MenuItems.Add(package1);
                context.MenuItems.Add(package2);

                await context.SaveChangesAsync();

                AddOptions(context, package1.Id, item.District, false);
                AddOptions(context, package2.Id, item.District, true);

                await context.SaveChangesAsync();
            }
        }

        private static void AddOptions(
            ApplicationDbContext context,
            int menuItemId,
            string district,
            bool premium)
        {
            var options = premium
                ? new List<MenuOption>
                {
                    new MenuOption { MenuItemId = menuItemId, OptionName = "Premium Table Setup", ExtraPrice = 1500 },
                    new MenuOption { MenuItemId = menuItemId, OptionName = "Professional Waiter Service", ExtraPrice = 2200 },
                    new MenuOption { MenuItemId = menuItemId, OptionName = $"{district} Local Dessert Selection", ExtraPrice = 900 },
                    new MenuOption { MenuItemId = menuItemId, OptionName = "Unlimited Soft Drinks", ExtraPrice = 1200 }
                }
                : new List<MenuOption>
                {
                    new MenuOption { MenuItemId = menuItemId, OptionName = "Rice Side Dish", ExtraPrice = 350 },
                    new MenuOption { MenuItemId = menuItemId, OptionName = "Salad Service", ExtraPrice = 300 },
                    new MenuOption { MenuItemId = menuItemId, OptionName = "Vegetarian Meal Alternative", ExtraPrice = 500 },
                    new MenuOption { MenuItemId = menuItemId, OptionName = "Dessert Add-on", ExtraPrice = 450 }
                };

            context.MenuOptions.AddRange(options);
        }

        private static string NormalizeEmailName(string value)
        {
            return value
                .ToLower()
                .Replace(" ", "")
                .Replace(".", "")
                .Replace("-", "")
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u");
        }
    }
}