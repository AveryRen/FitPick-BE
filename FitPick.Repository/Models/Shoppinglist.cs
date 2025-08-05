using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Shoppinglist
{
    public int Listid { get; set; }

    public int? Userid { get; set; }

    public DateOnly Date { get; set; }

    public string? Itemname { get; set; }

    public decimal? Quantity { get; set; }

    public string? Unit { get; set; }

    public virtual User? User { get; set; }
}
