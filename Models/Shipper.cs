using System;
using System.Collections.Generic;

namespace Ceng382_25_26_202211035.Models;

public partial class Shipper
{
    public int ShipperId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Phone { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public bool IsDeleted { get; set; } = false;
}
