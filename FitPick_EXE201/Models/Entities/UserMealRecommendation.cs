using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitPick_EXE201.Models.Entities
{
    [Table("user_meal_recommendations")]
    public class UserMealRecommendation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("mealid")]
        public int MealId { get; set; }

        [Column("recommendation_type")]
        [MaxLength(50)]
        public string RecommendationType { get; set; } = string.Empty;

        [Column("confidence_score", TypeName = "decimal(3,2)")]
        public decimal ConfidenceScore { get; set; } = 0.0m;

        [Column("reason")]
        public string? Reason { get; set; }

        [Column("algorithm_version")]
        [MaxLength(20)]
        public string AlgorithmVersion { get; set; } = "v1.0";

        [Column("is_viewed")]
        public bool IsViewed { get; set; } = false;

        [Column("is_accepted")]
        public bool? IsAccepted { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("MealId")]
        public virtual Meal? Meal { get; set; }
    }
}
