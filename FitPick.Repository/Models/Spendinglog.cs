using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Spendinglog
{
    public int Spendingid { get; set; }

    public int? Userid { get; set; }

    public DateOnly Date { get; set; }

    public decimal? Amount { get; set; }

    public string? Note { get; set; }

    public virtual User? User { get; set; }
}
