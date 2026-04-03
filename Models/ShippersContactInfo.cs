using System;
using System.Collections.Generic;

namespace Ceng382_25_26_202211035.Models;

public partial class ShippersContactInfo
{
    public int Id { get; set; }

    public int ShipperId { get; set; }

    public string Email { get; set; } = null!;

    public string Website { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string PostCode { get; set; } = null!;
}
