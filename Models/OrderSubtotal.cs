using System;
using System.Collections.Generic;

namespace Ceng382_25_26_202211035.Models;

public partial class OrderSubtotal
{
    public int OrderId { get; set; }

    public decimal? Subtotal { get; set; }
}
