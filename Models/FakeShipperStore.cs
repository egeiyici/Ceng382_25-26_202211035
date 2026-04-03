namespace Ceng382_25_26_202211035.Models;

public static class FakeShipperStore
{
    public static List<ShipperDisplayViewModel> Data { get; } =
    [
        new ShipperDisplayViewModel
        {
            Id = 1,
            ShipperId = 1,
            Email = "contact@bluerocket.com",
            Website = "www.bluerocket.com",
            Phone = "+1 212 555 1234",
            Address = "350 5th Ave",
            City = "New York",
            Country = "USA",
            PostCode = "10001",
            IsVisible = true
        },
        new ShipperDisplayViewModel
        {
            Id = 2,
            ShipperId = 2,
            Email = "info@moonline.de",
            Website = "www.moonline.de",
            Phone = "+49 30 123456",
            Address = "Alexanderplatz 1",
            City = "Berlin",
            Country = "Germany",
            PostCode = "10178",
            IsVisible = true
        },
        new ShipperDisplayViewModel
        {
            Id = 3,
            ShipperId = 3,
            Email = "support@crimsonturtle.co.uk",
            Website = "www.crimsonturtle.co.uk",
            Phone = "+44 20 7946 0958",
            Address = "221B Baker Street",
            City = "London",
            Country = "United Kingdom",
            PostCode = "NW1 6XE",
            IsVisible = true
        }
    ];
}