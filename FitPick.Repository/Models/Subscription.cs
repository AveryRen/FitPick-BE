using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Subscription
{
    public int Subscriptionid { get; set; }

    public int? Userid { get; set; }

    public DateTime Startdate { get; set; }

    public DateTime Enddate { get; set; }

    public decimal? Amount { get; set; }

    public virtual User? User { get; set; }
}
