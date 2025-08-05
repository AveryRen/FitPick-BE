using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Meal
{
    public int Mealid { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? Calories { get; set; }

    public int? Cookingtime { get; set; }

    public string? Diettype { get; set; }

    public decimal? Price { get; set; }

    public int? Createdby { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? CreatedbyNavigation { get; set; }

    public virtual ICollection<Favoritemeal> Favoritemeals { get; set; } = new List<Favoritemeal>();

    public virtual ICollection<Mealingredient> Mealingredients { get; set; } = new List<Mealingredient>();

    public virtual ICollection<Mealplan> Mealplans { get; set; } = new List<Mealplan>();
}
