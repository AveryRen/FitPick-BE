namespace FitPick_EXE201.Models.DTOs
{
    public class UserGoalDto
    {
        public int UserId { get; set; }
        public decimal? TargetWeight { get; set; }
        public int? TargetCalories { get; set; }

        public string? GoalName { get; set; }
    }
}
