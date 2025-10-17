using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IUserMealPreferenceRepository
    {
        Task<IEnumerable<UserMealPreferenceDto>> GetUserPreferencesAsync(int userId);
        Task<UserMealPreferenceDto?> GetUserPreferenceAsync(int userId, int mealId, string preferenceType);
        Task<UserMealPreferenceDto> CreatePreferenceAsync(int userId, CreateUserMealPreferenceDto dto);
        Task<UserMealPreferenceDto?> UpdatePreferenceAsync(int id, CreateUserMealPreferenceDto dto);
        Task<bool> DeletePreferenceAsync(int id);
        Task<IEnumerable<UserMealPreferenceDto>> GetMealPreferencesAsync(int mealId);
    }

    public interface IUserMealRatingRepository
    {
        Task<IEnumerable<UserMealRatingDto>> GetUserRatingsAsync(int userId);
        Task<UserMealRatingDto?> GetUserRatingAsync(int userId, int mealId);
        Task<UserMealRatingDto> CreateRatingAsync(int userId, CreateUserMealRatingDto dto);
        Task<UserMealRatingDto?> UpdateRatingAsync(int id, CreateUserMealRatingDto dto);
        Task<bool> DeleteRatingAsync(int id);
        Task<decimal> GetAverageRatingAsync(int mealId);
    }

    public interface IUserNutritionGoalRepository
    {
        Task<IEnumerable<UserNutritionGoalDto>> GetUserGoalsAsync(int userId);
        Task<UserNutritionGoalDto?> GetActiveGoalAsync(int userId, string goalType);
        Task<UserNutritionGoalDto> CreateGoalAsync(int userId, CreateUserNutritionGoalDto dto);
        Task<UserNutritionGoalDto?> UpdateGoalAsync(int id, CreateUserNutritionGoalDto dto);
        Task<bool> DeleteGoalAsync(int id);
        Task<bool> DeactivateGoalAsync(int id);
    }

    public interface IUserNutritionHistoryRepository
    {
        Task<IEnumerable<UserNutritionHistoryDto>> GetUserHistoryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<UserNutritionHistoryDto?> GetHistoryByDateAsync(int userId, DateTime date);
        Task<UserNutritionHistoryDto> CreateHistoryAsync(int userId, UserNutritionHistoryDto dto);
        Task<UserNutritionHistoryDto?> UpdateHistoryAsync(int id, UserNutritionHistoryDto dto);
        Task<bool> DeleteHistoryAsync(int id);
    }

    public interface IUserMealRecommendationRepository
    {
        Task<IEnumerable<UserMealRecommendationDto>> GetUserRecommendationsAsync(int userId, bool? isViewed = null);
        Task<UserMealRecommendationDto?> GetRecommendationAsync(int id);
        Task<UserMealRecommendationDto> CreateRecommendationAsync(int userId, int mealId, string recommendationType, decimal confidenceScore, string? reason = null);
        Task<UserMealRecommendationDto?> UpdateRecommendationAsync(int id, bool? isAccepted = null, bool? isViewed = null);
        Task<bool> DeleteRecommendationAsync(int id);
        Task<IEnumerable<UserMealRecommendationDto>> GetExpiredRecommendationsAsync();
    }

    public interface IUserMealExclusionRepository
    {
        Task<IEnumerable<UserMealExclusionDto>> GetUserExclusionsAsync(int userId);
        Task<UserMealExclusionDto?> GetExclusionAsync(int userId, int mealId, string exclusionType);
        Task<UserMealExclusionDto> CreateExclusionAsync(int userId, int mealId, string exclusionType, string? reason = null, bool isPermanent = true, DateTime? excludedUntil = null);
        Task<UserMealExclusionDto?> UpdateExclusionAsync(int id, string? reason = null, bool? isPermanent = null, DateTime? excludedUntil = null);
        Task<bool> DeleteExclusionAsync(int id);
        Task<bool> IsMealExcludedAsync(int userId, int mealId);
    }

    public interface IUserMealPatternRepository
    {
        Task<IEnumerable<UserMealPatternDto>> GetUserPatternsAsync(int userId);
        Task<UserMealPatternDto?> GetPatternAsync(int userId, string patternType, string patternValue);
        Task<UserMealPatternDto> CreatePatternAsync(int userId, string patternType, string patternValue, int frequencyCount = 1, decimal confidenceLevel = 0.0m);
        Task<UserMealPatternDto?> UpdatePatternAsync(int id, int? frequencyCount = null, decimal? confidenceLevel = null);
        Task<bool> DeletePatternAsync(int id);
        Task<IEnumerable<UserMealPatternDto>> GetTopPatternsAsync(int userId, string patternType, int limit = 10);
    }
}
