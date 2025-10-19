using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitPick_EXE201.Models.Entities
{
    [Table("user_meal_exclusions")]
    public class UserMealExclusion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("mealid")]
        public int MealId { get; set; }

        [Column("exclusion_type")]
        [MaxLength(50)]
        public string ExclusionType { get; set; } = string.Empty;

        [Column("exclusion_reason")]
        public string? ExclusionReason { get; set; }

        [Column("is_permanent")]
        public bool IsPermanent { get; set; } = true;

        [Column("excluded_until")]
        public DateTime? ExcludedUntil { get; set; }

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
