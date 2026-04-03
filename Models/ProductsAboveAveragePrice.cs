using System;
using System.Collections.Generic;

namespace Ceng382_25_26_202211035.Models;

public partial class ProductsAboveAveragePrice
{
    public string ProductName { get; set; } = null!;

    public decimal? UnitPrice { get; set; }
}
