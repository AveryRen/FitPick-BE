namespace FitPick_EXE201.Models.DTOs
{
    /// <summary>
    /// Gợi ý cá nhân hóa chuyên sâu cho PRO users
    /// </summary>
    public class DeepRecommendationDto
    {
        public string PersonalizedMessage { get; set; } = string.Empty;
        public List<RecommendedMealDto> RecommendedMeals { get; set; } = new();
        public List<string> NutritionTips { get; set; } = new();
        public List<string> GoalBasedAdvice { get; set; } = new();
        public ProgressSummary? Progress { get; set; }
    }

    public class RecommendedMealDto
    {
        public int MealId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal MatchScore { get; set; } // 0-100
        public string? ImageUrl { get; set; }
        public decimal? Calories { get; set; }
        public string MealTimeRecommendation { get; set; } = string.Empty; // breakfast, lunch, dinner
    }

    public class ProgressSummary
    {
        public string GoalName { get; set; } = string.Empty;
        public decimal CurrentWeight { get; set; }
        public decimal TargetWeight { get; set; }
        public decimal WeightChange { get; set; }
        public int DaysActive { get; set; }
        public decimal AverageCaloriesPerDay { get; set; }
        public string ProgressPercentage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Phân tích dinh dưỡng chi tiết
    /// </summary>
    public class NutritionInsightsDto
    {
        public int DaysAnalyzed { get; set; }
        public decimal AverageDailyCalories { get; set; }
        public decimal AverageDailyProtein { get; set; }
        public decimal AverageDailyCarbs { get; set; }
        public decimal AverageDailyFat { get; set; }
        public List<string> Insights { get; set; } = new();
        public List<NutrientTrendDto> Trends { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class NutrientTrendDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fat { get; set; }
    }

    /// <summary>
    /// Gợi ý món ăn theo thời gian trong ngày
    /// </summary>
    public class TimeBasedSuggestionsDto
    {
        public string CurrentMealTime { get; set; } = string.Empty; // breakfast, lunch, dinner, snack
        public List<MealSuggestionDto> Suggestions { get; set; } = new();
        public string ReasonForSuggestions { get; set; } = string.Empty;
    }

    public class MealSuggestionDto
    {
        public int MealId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Calories { get; set; }
        public string WhySuggested { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsQuickPrep { get; set; }
        public int CookingTime { get; set; }
    }
}
