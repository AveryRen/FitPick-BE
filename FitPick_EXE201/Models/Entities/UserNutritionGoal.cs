using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitPick_EXE201.Models.Entities
{
    [Table("user_nutrition_goals")]
    public class UserNutritionGoal
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("goal_type")]
        [MaxLength(50)]
        public string GoalType { get; set; } = string.Empty;

        [Column("target_calories")]
        public int? TargetCalories { get; set; }

        [Column("target_protein", TypeName = "decimal(8,2)")]
        public decimal? TargetProtein { get; set; }

        [Column("target_carbs", TypeName = "decimal(8,2)")]
        public decimal? TargetCarbs { get; set; }

        [Column("target_fat", TypeName = "decimal(8,2)")]
        public decimal? TargetFat { get; set; }

        [Column("target_fiber", TypeName = "decimal(8,2)")]
        public decimal? TargetFiber { get; set; }

        [Column("target_sugar", TypeName = "decimal(8,2)")]
        public decimal? TargetSugar { get; set; }

        [Column("target_sodium", TypeName = "decimal(8,2)")]
        public decimal? TargetSodium { get; set; }

        [Column("target_saturated_fat", TypeName = "decimal(8,2)")]
        public decimal? TargetSaturatedFat { get; set; }

        [Column("target_calcium", TypeName = "decimal(8,2)")]
        public decimal? TargetCalcium { get; set; }

        [Column("target_vitamin_d", TypeName = "decimal(8,2)")]
        public decimal? TargetVitaminD { get; set; }

        [Column("target_iron", TypeName = "decimal(8,2)")]
        public decimal? TargetIron { get; set; }

        [Column("target_vitamin_c", TypeName = "decimal(8,2)")]
        public decimal? TargetVitaminC { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
