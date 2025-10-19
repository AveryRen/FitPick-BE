using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitPick_EXE201.Models.Entities
{
    [Table("user_nutrition_history")]
    public class UserNutritionHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("consumed_calories")]
        public int ConsumedCalories { get; set; } = 0;

        [Column("consumed_protein", TypeName = "decimal(8,2)")]
        public decimal ConsumedProtein { get; set; } = 0;

        [Column("consumed_carbs", TypeName = "decimal(8,2)")]
        public decimal ConsumedCarbs { get; set; } = 0;

        [Column("consumed_fat", TypeName = "decimal(8,2)")]
        public decimal ConsumedFat { get; set; } = 0;

        [Column("consumed_fiber", TypeName = "decimal(8,2)")]
        public decimal ConsumedFiber { get; set; } = 0;

        [Column("consumed_sugar", TypeName = "decimal(8,2)")]
        public decimal ConsumedSugar { get; set; } = 0;

        [Column("consumed_sodium", TypeName = "decimal(8,2)")]
        public decimal ConsumedSodium { get; set; } = 0;

        [Column("consumed_saturated_fat", TypeName = "decimal(8,2)")]
        public decimal ConsumedSaturatedFat { get; set; } = 0;

        [Column("consumed_calcium", TypeName = "decimal(8,2)")]
        public decimal ConsumedCalcium { get; set; } = 0;

        [Column("consumed_vitamin_d", TypeName = "decimal(8,2)")]
        public decimal ConsumedVitaminD { get; set; } = 0;

        [Column("consumed_iron", TypeName = "decimal(8,2)")]
        public decimal ConsumedIron { get; set; } = 0;

        [Column("consumed_vitamin_c", TypeName = "decimal(8,2)")]
        public decimal ConsumedVitaminC { get; set; } = 0;

        [Column("meal_count")]
        public int MealCount { get; set; } = 0;

        [Column("water_intake", TypeName = "decimal(6,2)")]
        public decimal WaterIntake { get; set; } = 0;

        [Column("exercise_calories")]
        public int ExerciseCalories { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
