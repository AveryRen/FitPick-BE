namespace FitPick_EXE201.Models.DTOs
{
    public class AIRequestDto
    {
        public int UserId { get; set; }
        public UserProfileDto Profile { get; set; }
        public List<MealHistoryDto> MealHistory { get; set; }
        public List<MealRecommendationDto> AvailableMeals { get; set; }
    }
}