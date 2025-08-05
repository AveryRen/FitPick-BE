using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Mealingredient
{
    public int Id { get; set; }

    public int? Mealid { get; set; }

    public int? Ingredientid { get; set; }

    public decimal? Quantity { get; set; }

    public virtual Ingredient? Ingredient { get; set; }

    public virtual Meal? Meal { get; set; }
}
