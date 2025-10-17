using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public interface IPersonalizationService
    {
        // Meal Preferences
        Task<IEnumerable<UserMealPreferenceDto>> GetUserPreferencesAsync(int userId);
        Task<UserMealPreferenceDto> CreatePreferenceAsync(int userId, CreateUserMealPreferenceDto dto);
        Task<UserMealPreferenceDto?> UpdatePreferenceAsync(int id, CreateUserMealPreferenceDto dto);
        Task<bool> DeletePreferenceAsync(int id);

        // Meal Ratings
        Task<IEnumerable<UserMealRatingDto>> GetUserRatingsAsync(int userId);
        Task<UserMealRatingDto> CreateRatingAsync(int userId, CreateUserMealRatingDto dto);
        Task<UserMealRatingDto?> UpdateRatingAsync(int id, CreateUserMealRatingDto dto);
        Task<bool> DeleteRatingAsync(int id);
        Task<decimal> GetAverageRatingAsync(int mealId);

        // Nutrition Goals
        Task<IEnumerable<UserNutritionGoalDto>> GetUserGoalsAsync(int userId);
        Task<UserNutritionGoalDto?> GetActiveGoalAsync(int userId, string goalType);
        Task<UserNutritionGoalDto> CreateGoalAsync(int userId, CreateUserNutritionGoalDto dto);
        Task<UserNutritionGoalDto?> UpdateGoalAsync(int id, CreateUserNutritionGoalDto dto);

        // Nutrition History
        Task<IEnumerable<UserNutritionHistoryDto>> GetUserHistoryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<UserNutritionHistoryDto?> GetHistoryByDateAsync(int userId, DateTime date);
        Task<UserNutritionHistoryDto> CreateHistoryAsync(int userId, UserNutritionHistoryDto dto);

        // Meal Recommendations
        Task<IEnumerable<UserMealRecommendationDto>> GetUserRecommendationsAsync(int userId, bool? isViewed = null);
        Task<UserMealRecommendationDto> CreateRecommendationAsync(int userId, int mealId, string recommendationType, decimal confidenceScore, string? reason = null);
        Task<UserMealRecommendationDto?> UpdateRecommendationAsync(int id, bool? isAccepted = null, bool? isViewed = null);

        // Meal Exclusions
        Task<IEnumerable<UserMealExclusionDto>> GetUserExclusionsAsync(int userId);
        Task<UserMealExclusionDto> CreateExclusionAsync(int userId, int mealId, string exclusionType, string? reason = null, bool isPermanent = true, DateTime? excludedUntil = null);
        Task<bool> DeleteExclusionAsync(int id);
        Task<bool> IsMealExcludedAsync(int userId, int mealId);

        // Meal Patterns
        Task<IEnumerable<UserMealPatternDto>> GetUserPatternsAsync(int userId);
        Task<UserMealPatternDto> CreatePatternAsync(int userId, string patternType, string patternValue, int frequencyCount = 1, decimal confidenceLevel = 0.0m);
        Task<IEnumerable<UserMealPatternDto>> GetTopPatternsAsync(int userId, string patternType, int limit = 10);

        // AI Recommendation Engine
        Task<IEnumerable<UserMealRecommendationDto>> GenerateRecommendationsAsync(int userId, int limit = 10);
        Task UpdateUserPatternsAsync(int userId, int mealId, string action); // action: "like", "dislike", "cook", "skip"
    }
}
