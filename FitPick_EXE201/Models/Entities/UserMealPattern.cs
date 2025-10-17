using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitPick_EXE201.Models.Entities
{
    [Table("user_meal_patterns")]
    public class UserMealPattern
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("pattern_type")]
        [MaxLength(50)]
        public string PatternType { get; set; } = string.Empty;

        [Column("pattern_value")]
        [MaxLength(100)]
        public string PatternValue { get; set; } = string.Empty;

        [Column("frequency_count")]
        public int FrequencyCount { get; set; } = 1;

        [Column("confidence_level", TypeName = "decimal(3,2)")]
        public decimal ConfidenceLevel { get; set; } = 0.0m;

        [Column("last_occurrence")]
        public DateTime LastOccurrence { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
