using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitPick_EXE201.Models.Entities
{
    [Table("user_meal_ratings")]
    public class UserMealRating
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("mealid")]
        public int MealId { get; set; }

        [Column("rating", TypeName = "decimal(2,1)")]
        [Range(1.0, 5.0)]
        public decimal Rating { get; set; }

        [Column("review_text")]
        public string? ReviewText { get; set; }

        [Column("taste_rating", TypeName = "decimal(2,1)")]
        [Range(1.0, 5.0)]
        public decimal? TasteRating { get; set; }

        [Column("health_rating", TypeName = "decimal(2,1)")]
        [Range(1.0, 5.0)]
        public decimal? HealthRating { get; set; }

        [Column("difficulty_rating", TypeName = "decimal(2,1)")]
        [Range(1.0, 5.0)]
        public decimal? DifficultyRating { get; set; }

        [Column("would_cook_again")]
        public bool WouldCookAgain { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("MealId")]
        public virtual Meal? Meal { get; set; }
    }
}
