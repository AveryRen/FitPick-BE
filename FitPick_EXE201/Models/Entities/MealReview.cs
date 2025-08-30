using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("meal_reviews")]
[Index("Mealid", "Userid", Name = "uq_meal_user", IsUnique = true)]
public partial class MealReview
{
    [Key]
    [Column("reviewid")]
    public int Reviewid { get; set; }

    [Column("mealid")]
    public int Mealid { get; set; }

    [Column("userid")]
    public int Userid { get; set; }

    [Column("rating")]
    public int? Rating { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("is_favorite")]
    public bool? IsFavorite { get; set; }

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [Column("updatedat", TypeName = "timestamp without time zone")]
    public DateTime? Updatedat { get; set; }

    [ForeignKey("Mealid")]
    [InverseProperty("MealReviews")]
    public virtual Meal Meal { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("MealReviews")]
    public virtual User User { get; set; } = null!;
}
