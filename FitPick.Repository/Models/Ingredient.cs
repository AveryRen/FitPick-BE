using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Ingredient
{
    public int Ingredientid { get; set; }

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public string? Unit { get; set; }

    public virtual ICollection<Locationingredient> Locationingredients { get; set; } = new List<Locationingredient>();

    public virtual ICollection<Mealingredient> Mealingredients { get; set; } = new List<Mealingredient>();
}
