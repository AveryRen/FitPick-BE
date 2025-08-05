using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Locationingredient
{
    public int Id { get; set; }

    public int? Locationid { get; set; }

    public int? Ingredientid { get; set; }

    public decimal? Price { get; set; }

    public bool? Availability { get; set; }

    public virtual Ingredient? Ingredient { get; set; }

    public virtual Location? Location { get; set; }
}
