using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("ingredients")]
public partial class Ingredient
{
    [Key]
    [Column("ingredientid")]
    public int Ingredientid { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("type")]
    [StringLength(50)]
    public string? Type { get; set; }

    [Column("unit")]
    [StringLength(20)]
    public string? Unit { get; set; }

    [Column("status")]
    public bool? Status { get; set; }

    [InverseProperty("Ingredient")]
    public virtual ICollection<Mealingredient> Mealingredients { get; set; } = new List<Mealingredient>();
}
