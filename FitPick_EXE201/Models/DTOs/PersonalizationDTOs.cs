namespace FitPick_EXE201.Models.DTOs
{
    public class UserMealPreferenceDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MealId { get; set; }
        public string PreferenceType { get; set; } = string.Empty;
        public decimal PreferenceScore { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateUserMealPreferenceDto
    {
        public int MealId { get; set; }
        public string PreferenceType { get; set; } = string.Empty;
        public decimal PreferenceScore { get; set; }
        public string? Reason { get; set; }
    }

    public class UserMealRatingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MealId { get; set; }
        public decimal Rating { get; set; }
        public string? ReviewText { get; set; }
        public decimal? TasteRating { get; set; }
        public decimal? HealthRating { get; set; }
        public decimal? DifficultyRating { get; set; }
        public bool WouldCookAgain { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateUserMealRatingDto
    {
        public int MealId { get; set; }
        public decimal Rating { get; set; }
        public string? ReviewText { get; set; }
        public decimal? TasteRating { get; set; }
        public decimal? HealthRating { get; set; }
        public decimal? DifficultyRating { get; set; }
        public bool WouldCookAgain { get; set; }
    }

    public class UserNutritionGoalDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string GoalType { get; set; } = string.Empty;
        public int? TargetCalories { get; set; }
        public decimal? TargetProtein { get; set; }
        public decimal? TargetCarbs { get; set; }
        public decimal? TargetFat { get; set; }
        public decimal? TargetFiber { get; set; }
        public decimal? TargetSugar { get; set; }
        public decimal? TargetSodium { get; set; }
        public decimal? TargetSaturatedFat { get; set; }
        public decimal? TargetCalcium { get; set; }
        public decimal? TargetVitaminD { get; set; }
        public decimal? TargetIron { get; set; }
        public decimal? TargetVitaminC { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateUserNutritionGoalDto
    {
        public string GoalType { get; set; } = string.Empty;
        public int? TargetCalories { get; set; }
        public decimal? TargetProtein { get; set; }
        public decimal? TargetCarbs { get; set; }
        public decimal? TargetFat { get; set; }
        public decimal? TargetFiber { get; set; }
        public decimal? TargetSugar { get; set; }
        public decimal? TargetSodium { get; set; }
        public decimal? TargetSaturatedFat { get; set; }
        public decimal? TargetCalcium { get; set; }
        public decimal? TargetVitaminD { get; set; }
        public decimal? TargetIron { get; set; }
        public decimal? TargetVitaminC { get; set; }
    }

    public class UserNutritionHistoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public int ConsumedCalories { get; set; }
        public decimal ConsumedProtein { get; set; }
        public decimal ConsumedCarbs { get; set; }
        public decimal ConsumedFat { get; set; }
        public decimal ConsumedFiber { get; set; }
        public decimal ConsumedSugar { get; set; }
        public decimal ConsumedSodium { get; set; }
        public decimal ConsumedSaturatedFat { get; set; }
        public decimal ConsumedCalcium { get; set; }
        public decimal ConsumedVitaminD { get; set; }
        public decimal ConsumedIron { get; set; }
        public decimal ConsumedVitaminC { get; set; }
        public int MealCount { get; set; }
        public decimal WaterIntake { get; set; }
        public int ExerciseCalories { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserMealRecommendationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MealId { get; set; }
        public string RecommendationType { get; set; } = string.Empty;
        public decimal ConfidenceScore { get; set; }
        public string? Reason { get; set; }
        public string AlgorithmVersion { get; set; } = string.Empty;
        public bool IsViewed { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class UserMealExclusionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MealId { get; set; }
        public string ExclusionType { get; set; } = string.Empty;
        public string? ExclusionReason { get; set; }
        public bool IsPermanent { get; set; }
        public DateTime? ExcludedUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserMealPatternDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PatternType { get; set; } = string.Empty;
        public string PatternValue { get; set; } = string.Empty;
        public int FrequencyCount { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public DateTime LastOccurrence { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateMealExclusionDto
    {
        public int MealId { get; set; }
        public string ExclusionType { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public bool IsPermanent { get; set; } = true;
        public DateTime? ExcludedUntil { get; set; }
    }
}
