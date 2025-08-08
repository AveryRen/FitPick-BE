using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("mealingredients")]
public partial class Mealingredient
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("mealid")]
    public int? Mealid { get; set; }

    [Column("ingredientid")]
    public int? Ingredientid { get; set; }

    [Column("quantity")]
    [Precision(10, 2)]
    public decimal? Quantity { get; set; }

    [ForeignKey("Ingredientid")]
    [InverseProperty("Mealingredients")]
    public virtual Ingredient? Ingredient { get; set; }

    [ForeignKey("Mealid")]
    [InverseProperty("Mealingredients")]
    public virtual Meal? Meal { get; set; }
}
