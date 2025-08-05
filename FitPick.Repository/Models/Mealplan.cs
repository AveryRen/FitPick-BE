using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Mealplan
{
    public int Planid { get; set; }

    public int? Userid { get; set; }

    public DateOnly Date { get; set; }

    public int? Mealid { get; set; }

    public virtual Meal? Meal { get; set; }

    public virtual User? User { get; set; }
}
