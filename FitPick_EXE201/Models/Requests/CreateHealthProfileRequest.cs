using System.ComponentModel.DataAnnotations;

namespace FitPick_EXE201.Models.Requests
{
    public class CreateHealthProfileRequest
    {
        [Required]
        public int HealthGoalId { get; set; }
        
        [Required]
        public int LifestyleId { get; set; }
        
        public int? TargetCalories { get; set; }
        
        public decimal? TargetWeight { get; set; }
        
        public int? DailyMeals { get; set; }
    }
}
