namespace FitPick_EXE201.Models.DTOs
{
    public class WeeklyMealPlanRequest
    {
        public string WeekStartDate { get; set; } = string.Empty;
        public string? HealthGoal { get; set; }
        public string? Lifestyle { get; set; }
        public string? DietType { get; set; }
        public bool IncludeBreakfast { get; set; } = true;
        public bool IncludeLunch { get; set; } = true;
        public bool IncludeDinner { get; set; } = true;
        public bool IncludeSnacks { get; set; } = false;
    }
}
