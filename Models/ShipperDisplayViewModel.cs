namespace Ceng382_25_26_202211035.Models;

public class ShipperDisplayViewModel
{
    public int Id { get; set; }
    public int ShipperId { get; set; }
    public string Email { get; set; } = "";
    public string Website { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
    public string PostCode { get; set; } = "";

    public bool IsVisible { get; set; } = true;
}