using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("meals")]
public partial class Meal
{
    [Key]
    [Column("mealid")]
    public int Mealid { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("calories")]
    public int? Calories { get; set; }

    [Column("cookingtime")]
    public int? Cookingtime { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("diettype")]
    [StringLength(50)]
    public string? Diettype { get; set; }

    [Column("price")]
    [Precision(10, 2)]
    public decimal? Price { get; set; }

    [Column("status_id")]
    public int? StatusId { get; set; }

    [Column("createdby")]
    public int? Createdby { get; set; }

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Meals")]
    public virtual MealCategory? Category { get; set; }

    [ForeignKey("Createdby")]
    [InverseProperty("Meals")]
    public virtual User? CreatedbyNavigation { get; set; }

    [InverseProperty("Meal")]
    public virtual ICollection<Favoritemeal> Favoritemeals { get; set; } = new List<Favoritemeal>();

    [InverseProperty("Meal")]
    public virtual ICollection<Mealingredient> Mealingredients { get; set; } = new List<Mealingredient>();

    [InverseProperty("Meal")]
    public virtual ICollection<Mealplan> Mealplans { get; set; } = new List<Mealplan>();

    [ForeignKey("StatusId")]
    [InverseProperty("Meals")]
    public virtual MealStatus? Status { get; set; }
}
