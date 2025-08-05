using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Favoritemeal
{
    public int Favoriteid { get; set; }

    public int? Userid { get; set; }

    public int? Mealid { get; set; }

    public int? Rating { get; set; }

    public string? Note { get; set; }

    public virtual Meal? Meal { get; set; }

    public virtual User? User { get; set; }
}
