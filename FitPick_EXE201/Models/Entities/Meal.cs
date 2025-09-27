using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
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

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [Column("is_premium")]
    public bool? IsPremium { get; set; }

    [Column("image_url")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Column("protein")]
    [Precision(6, 2)]
    public decimal? Protein { get; set; }

    [Column("carbs")]
    [Precision(6, 2)]
    public decimal? Carbs { get; set; }

    [Column("fat")]
    [Precision(6, 2)]
    public decimal? Fat { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Meals")]
    [JsonIgnore]
    public virtual MealCategory? Category { get; set; }

    [InverseProperty("Meal")]
    public virtual ICollection<MealHistory> MealHistories { get; set; } = new List<MealHistory>();

    [InverseProperty("Meal")]
    public virtual ICollection<MealInstruction> MealInstructions { get; set; } = new List<MealInstruction>();

    [InverseProperty("Meal")]
    [JsonIgnore]
    public virtual ICollection<MealReview> MealReviews { get; set; } = new List<MealReview>();

    [InverseProperty("Meal")]
    [JsonIgnore]
    public virtual ICollection<Mealingredient> Mealingredients { get; set; } = new List<Mealingredient>();

    [InverseProperty("Meal")]
    [JsonIgnore]
    public virtual ICollection<Mealplan> Mealplans { get; set; } = new List<Mealplan>();

    [ForeignKey("StatusId")]
    [InverseProperty("Meals")]
    public virtual MealStatus? Status { get; set; }

    [InverseProperty("Meal")]
    public virtual ICollection<UserMealIngredientMark> UserMealIngredientMarks { get; set; } = new List<UserMealIngredientMark>();
}
