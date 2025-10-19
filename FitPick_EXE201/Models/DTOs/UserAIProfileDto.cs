using System.ComponentModel;

namespace FitPick_EXE201.Models.DTOs
{
    public class UserAIProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? HealthGoal { get; set; }
        public string? Lifestyle { get; set; }
        public List<int>? Allergies { get; set; }
        public List<int>? Religiondiet { get; set; }
        public List<int>? Dietarypreferences { get; set; }

        // N?u mu?n gi? DietPreferences riêng bi?t
        public List<int>? DietPreferences
        {
            get => Dietarypreferences;
            set => Dietarypreferences = value;
        }

        public decimal? ProgressPercent { get; set; }
        public int? TargetCalories { get; set; }
    }
}
