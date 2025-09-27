using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("user_meal_ingredient_marks")]
[Index("Userid", "Mealid", "Ingredientid", Name = "uq_user_meal_ing", IsUnique = true)]
public partial class UserMealIngredientMark
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("userid")]
    public int Userid { get; set; }

    [Column("mealid")]
    public int Mealid { get; set; }

    [Column("ingredientid")]
    public int Ingredientid { get; set; }

    [Column("has_it")]
    public bool? HasIt { get; set; }

    [Column("quantity")]
    [Precision(10, 2)]
    public decimal? Quantity { get; set; }

    [Column("unit")]
    [StringLength(20)]
    public string? Unit { get; set; }

    [ForeignKey("Ingredientid")]
    [InverseProperty("UserMealIngredientMarks")]
    public virtual Ingredient Ingredient { get; set; } = null!;

    [ForeignKey("Mealid")]
    [InverseProperty("UserMealIngredientMarks")]
    public virtual Meal Meal { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("UserMealIngredientMarks")]
    public virtual User User { get; set; } = null!;
}
