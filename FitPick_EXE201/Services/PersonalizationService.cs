using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class PersonalizationService : IPersonalizationService
    {
        private readonly FitPickContext _context;

        public PersonalizationService(FitPickContext context)
        {
            _context = context;
        }

        // Meal Preferences
        public async Task<IEnumerable<UserMealPreferenceDto>> GetUserPreferencesAsync(int userId)
        {
            var preferences = await _context.UserMealPreferences
                .Where(p => p.UserId == userId)
                .Select(p => new UserMealPreferenceDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    MealId = p.MealId,
                    PreferenceType = p.PreferenceType,
                    PreferenceScore = p.PreferenceScore,
                    Reason = p.Reason,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return preferences;
        }

        public async Task<UserMealPreferenceDto> CreatePreferenceAsync(int userId, CreateUserMealPreferenceDto dto)
        {
            var preference = new UserMealPreference
            {
                UserId = userId,
                MealId = dto.MealId,
                PreferenceType = dto.PreferenceType,
                PreferenceScore = dto.PreferenceScore,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserMealPreferences.Add(preference);
            await _context.SaveChangesAsync();

            return new UserMealPreferenceDto
            {
                Id = preference.Id,
                UserId = preference.UserId,
                MealId = preference.MealId,
                PreferenceType = preference.PreferenceType,
                PreferenceScore = preference.PreferenceScore,
                Reason = preference.Reason,
                CreatedAt = preference.CreatedAt,
                UpdatedAt = preference.UpdatedAt
            };
        }

        public async Task<UserMealPreferenceDto?> UpdatePreferenceAsync(int id, CreateUserMealPreferenceDto dto)
        {
            var preference = await _context.UserMealPreferences.FindAsync(id);
            if (preference == null) return null;

            preference.MealId = dto.MealId;
            preference.PreferenceType = dto.PreferenceType;
            preference.PreferenceScore = dto.PreferenceScore;
            preference.Reason = dto.Reason;
            preference.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserMealPreferenceDto
            {
                Id = preference.Id,
                UserId = preference.UserId,
                MealId = preference.MealId,
                PreferenceType = preference.PreferenceType,
                PreferenceScore = preference.PreferenceScore,
                Reason = preference.Reason,
                CreatedAt = preference.CreatedAt,
                UpdatedAt = preference.UpdatedAt
            };
        }

        public async Task<bool> DeletePreferenceAsync(int id)
        {
            var preference = await _context.UserMealPreferences.FindAsync(id);
            if (preference == null) return false;

            _context.UserMealPreferences.Remove(preference);
            await _context.SaveChangesAsync();
            return true;
        }

        // Meal Ratings
        public async Task<IEnumerable<UserMealRatingDto>> GetUserRatingsAsync(int userId)
        {
            var ratings = await _context.UserMealRatings
                .Where(r => r.UserId == userId)
                .Select(r => new UserMealRatingDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    MealId = r.MealId,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    TasteRating = r.TasteRating,
                    HealthRating = r.HealthRating,
                    DifficultyRating = r.DifficultyRating,
                    WouldCookAgain = r.WouldCookAgain,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return ratings;
        }

        public async Task<UserMealRatingDto> CreateRatingAsync(int userId, CreateUserMealRatingDto dto)
        {
            var rating = new UserMealRating
            {
                UserId = userId,
                MealId = dto.MealId,
                Rating = dto.Rating,
                ReviewText = dto.ReviewText,
                TasteRating = dto.TasteRating,
                HealthRating = dto.HealthRating,
                DifficultyRating = dto.DifficultyRating,
                WouldCookAgain = dto.WouldCookAgain,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserMealRatings.Add(rating);
            await _context.SaveChangesAsync();

            return new UserMealRatingDto
            {
                Id = rating.Id,
                UserId = rating.UserId,
                MealId = rating.MealId,
                Rating = rating.Rating,
                ReviewText = rating.ReviewText,
                TasteRating = rating.TasteRating,
                HealthRating = rating.HealthRating,
                DifficultyRating = rating.DifficultyRating,
                WouldCookAgain = rating.WouldCookAgain,
                CreatedAt = rating.CreatedAt,
                UpdatedAt = rating.UpdatedAt
            };
        }

        public async Task<UserMealRatingDto?> UpdateRatingAsync(int id, CreateUserMealRatingDto dto)
        {
            var rating = await _context.UserMealRatings.FindAsync(id);
            if (rating == null) return null;

            rating.MealId = dto.MealId;
            rating.Rating = dto.Rating;
            rating.ReviewText = dto.ReviewText;
            rating.TasteRating = dto.TasteRating;
            rating.HealthRating = dto.HealthRating;
            rating.DifficultyRating = dto.DifficultyRating;
            rating.WouldCookAgain = dto.WouldCookAgain;
            rating.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserMealRatingDto
            {
                Id = rating.Id,
                UserId = rating.UserId,
                MealId = rating.MealId,
                Rating = rating.Rating,
                ReviewText = rating.ReviewText,
                TasteRating = rating.TasteRating,
                HealthRating = rating.HealthRating,
                DifficultyRating = rating.DifficultyRating,
                WouldCookAgain = rating.WouldCookAgain,
                CreatedAt = rating.CreatedAt,
                UpdatedAt = rating.UpdatedAt
            };
        }

        public async Task<bool> DeleteRatingAsync(int id)
        {
            var rating = await _context.UserMealRatings.FindAsync(id);
            if (rating == null) return false;

            _context.UserMealRatings.Remove(rating);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetAverageRatingAsync(int mealId)
        {
            var averageRating = await _context.UserMealRatings
                .Where(r => r.MealId == mealId)
                .AverageAsync(r => r.Rating);

            return averageRating;
        }

        // Nutrition Goals
        public async Task<IEnumerable<UserNutritionGoalDto>> GetUserGoalsAsync(int userId)
        {
            var goals = await _context.UserNutritionGoals
                .Where(g => g.UserId == userId)
                .Select(g => new UserNutritionGoalDto
                {
                    Id = g.Id,
                    UserId = g.UserId,
                    GoalType = g.GoalType,
                    TargetCalories = g.TargetCalories,
                    TargetProtein = g.TargetProtein,
                    TargetCarbs = g.TargetCarbs,
                    TargetFat = g.TargetFat,
                    TargetFiber = g.TargetFiber,
                    TargetSugar = g.TargetSugar,
                    TargetSodium = g.TargetSodium,
                    TargetSaturatedFat = g.TargetSaturatedFat,
                    TargetCalcium = g.TargetCalcium,
                    TargetVitaminD = g.TargetVitaminD,
                    TargetIron = g.TargetIron,
                    TargetVitaminC = g.TargetVitaminC,
                    IsActive = g.IsActive,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();

            return goals;
        }

        public async Task<UserNutritionGoalDto?> GetActiveGoalAsync(int userId, string goalType)
        {
            var goal = await _context.UserNutritionGoals
                .Where(g => g.UserId == userId && g.GoalType == goalType && g.IsActive)
                .Select(g => new UserNutritionGoalDto
                {
                    Id = g.Id,
                    UserId = g.UserId,
                    GoalType = g.GoalType,
                    TargetCalories = g.TargetCalories,
                    TargetProtein = g.TargetProtein,
                    TargetCarbs = g.TargetCarbs,
                    TargetFat = g.TargetFat,
                    TargetFiber = g.TargetFiber,
                    TargetSugar = g.TargetSugar,
                    TargetSodium = g.TargetSodium,
                    TargetSaturatedFat = g.TargetSaturatedFat,
                    TargetCalcium = g.TargetCalcium,
                    TargetVitaminD = g.TargetVitaminD,
                    TargetIron = g.TargetIron,
                    TargetVitaminC = g.TargetVitaminC,
                    IsActive = g.IsActive,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return goal;
        }

        public async Task<UserNutritionGoalDto> CreateGoalAsync(int userId, CreateUserNutritionGoalDto dto)
        {
            var goal = new UserNutritionGoal
            {
                UserId = userId,
                GoalType = dto.GoalType,
                TargetCalories = dto.TargetCalories,
                TargetProtein = dto.TargetProtein,
                TargetCarbs = dto.TargetCarbs,
                TargetFat = dto.TargetFat,
                TargetFiber = dto.TargetFiber,
                TargetSugar = dto.TargetSugar,
                TargetSodium = dto.TargetSodium,
                TargetSaturatedFat = dto.TargetSaturatedFat,
                TargetCalcium = dto.TargetCalcium,
                TargetVitaminD = dto.TargetVitaminD,
                TargetIron = dto.TargetIron,
                TargetVitaminC = dto.TargetVitaminC,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserNutritionGoals.Add(goal);
            await _context.SaveChangesAsync();

            return new UserNutritionGoalDto
            {
                Id = goal.Id,
                UserId = goal.UserId,
                GoalType = goal.GoalType,
                TargetCalories = goal.TargetCalories,
                TargetProtein = goal.TargetProtein,
                TargetCarbs = goal.TargetCarbs,
                TargetFat = goal.TargetFat,
                TargetFiber = goal.TargetFiber,
                TargetSugar = goal.TargetSugar,
                TargetSodium = goal.TargetSodium,
                TargetSaturatedFat = goal.TargetSaturatedFat,
                TargetCalcium = goal.TargetCalcium,
                TargetVitaminD = goal.TargetVitaminD,
                TargetIron = goal.TargetIron,
                TargetVitaminC = goal.TargetVitaminC,
                IsActive = goal.IsActive,
                CreatedAt = goal.CreatedAt,
                UpdatedAt = goal.UpdatedAt
            };
        }

        public async Task<UserNutritionGoalDto?> UpdateGoalAsync(int id, CreateUserNutritionGoalDto dto)
        {
            var goal = await _context.UserNutritionGoals.FindAsync(id);
            if (goal == null) return null;

            goal.GoalType = dto.GoalType;
            goal.TargetCalories = dto.TargetCalories;
            goal.TargetProtein = dto.TargetProtein;
            goal.TargetCarbs = dto.TargetCarbs;
            goal.TargetFat = dto.TargetFat;
            goal.TargetFiber = dto.TargetFiber;
            goal.TargetSugar = dto.TargetSugar;
            goal.TargetSodium = dto.TargetSodium;
            goal.TargetSaturatedFat = dto.TargetSaturatedFat;
            goal.TargetCalcium = dto.TargetCalcium;
            goal.TargetVitaminD = dto.TargetVitaminD;
            goal.TargetIron = dto.TargetIron;
            goal.TargetVitaminC = dto.TargetVitaminC;
            goal.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserNutritionGoalDto
            {
                Id = goal.Id,
                UserId = goal.UserId,
                GoalType = goal.GoalType,
                TargetCalories = goal.TargetCalories,
                TargetProtein = goal.TargetProtein,
                TargetCarbs = goal.TargetCarbs,
                TargetFat = goal.TargetFat,
                TargetFiber = goal.TargetFiber,
                TargetSugar = goal.TargetSugar,
                TargetSodium = goal.TargetSodium,
                TargetSaturatedFat = goal.TargetSaturatedFat,
                TargetCalcium = goal.TargetCalcium,
                TargetVitaminD = goal.TargetVitaminD,
                TargetIron = goal.TargetIron,
                TargetVitaminC = goal.TargetVitaminC,
                IsActive = goal.IsActive,
                CreatedAt = goal.CreatedAt,
                UpdatedAt = goal.UpdatedAt
            };
        }

        // Nutrition History
        public async Task<IEnumerable<UserNutritionHistoryDto>> GetUserHistoryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.UserNutritionHistories.Where(h => h.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(h => h.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(h => h.Date <= endDate.Value);

            var history = await query
                .Select(h => new UserNutritionHistoryDto
                {
                    Id = h.Id,
                    UserId = h.UserId,
                    Date = h.Date,
                    ConsumedCalories = h.ConsumedCalories,
                    ConsumedProtein = h.ConsumedProtein,
                    ConsumedCarbs = h.ConsumedCarbs,
                    ConsumedFat = h.ConsumedFat,
                    ConsumedFiber = h.ConsumedFiber,
                    ConsumedSugar = h.ConsumedSugar,
                    ConsumedSodium = h.ConsumedSodium,
                    ConsumedSaturatedFat = h.ConsumedSaturatedFat,
                    ConsumedCalcium = h.ConsumedCalcium,
                    ConsumedVitaminD = h.ConsumedVitaminD,
                    ConsumedIron = h.ConsumedIron,
                    ConsumedVitaminC = h.ConsumedVitaminC,
                    MealCount = h.MealCount,
                    WaterIntake = h.WaterIntake,
                    ExerciseCalories = h.ExerciseCalories,
                    CreatedAt = h.CreatedAt,
                    UpdatedAt = h.UpdatedAt
                })
                .ToListAsync();

            return history;
        }

        public async Task<UserNutritionHistoryDto?> GetHistoryByDateAsync(int userId, DateTime date)
        {
            var history = await _context.UserNutritionHistories
                .Where(h => h.UserId == userId && h.Date.Date == date.Date)
                .Select(h => new UserNutritionHistoryDto
                {
                    Id = h.Id,
                    UserId = h.UserId,
                    Date = h.Date,
                    ConsumedCalories = h.ConsumedCalories,
                    ConsumedProtein = h.ConsumedProtein,
                    ConsumedCarbs = h.ConsumedCarbs,
                    ConsumedFat = h.ConsumedFat,
                    ConsumedFiber = h.ConsumedFiber,
                    ConsumedSugar = h.ConsumedSugar,
                    ConsumedSodium = h.ConsumedSodium,
                    ConsumedSaturatedFat = h.ConsumedSaturatedFat,
                    ConsumedCalcium = h.ConsumedCalcium,
                    ConsumedVitaminD = h.ConsumedVitaminD,
                    ConsumedIron = h.ConsumedIron,
                    ConsumedVitaminC = h.ConsumedVitaminC,
                    MealCount = h.MealCount,
                    WaterIntake = h.WaterIntake,
                    ExerciseCalories = h.ExerciseCalories,
                    CreatedAt = h.CreatedAt,
                    UpdatedAt = h.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return history;
        }

        public async Task<UserNutritionHistoryDto> CreateHistoryAsync(int userId, UserNutritionHistoryDto dto)
        {
            var history = new UserNutritionHistory
            {
                UserId = userId,
                Date = dto.Date,
                ConsumedCalories = dto.ConsumedCalories,
                ConsumedProtein = dto.ConsumedProtein,
                ConsumedCarbs = dto.ConsumedCarbs,
                ConsumedFat = dto.ConsumedFat,
                ConsumedFiber = dto.ConsumedFiber,
                ConsumedSugar = dto.ConsumedSugar,
                ConsumedSodium = dto.ConsumedSodium,
                ConsumedSaturatedFat = dto.ConsumedSaturatedFat,
                ConsumedCalcium = dto.ConsumedCalcium,
                ConsumedVitaminD = dto.ConsumedVitaminD,
                ConsumedIron = dto.ConsumedIron,
                ConsumedVitaminC = dto.ConsumedVitaminC,
                MealCount = dto.MealCount,
                WaterIntake = dto.WaterIntake,
                ExerciseCalories = dto.ExerciseCalories,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserNutritionHistories.Add(history);
            await _context.SaveChangesAsync();

            return new UserNutritionHistoryDto
            {
                Id = history.Id,
                UserId = history.UserId,
                Date = history.Date,
                ConsumedCalories = history.ConsumedCalories,
                ConsumedProtein = history.ConsumedProtein,
                ConsumedCarbs = history.ConsumedCarbs,
                ConsumedFat = history.ConsumedFat,
                ConsumedFiber = history.ConsumedFiber,
                ConsumedSugar = history.ConsumedSugar,
                ConsumedSodium = history.ConsumedSodium,
                ConsumedSaturatedFat = history.ConsumedSaturatedFat,
                ConsumedCalcium = history.ConsumedCalcium,
                ConsumedVitaminD = history.ConsumedVitaminD,
                ConsumedIron = history.ConsumedIron,
                ConsumedVitaminC = history.ConsumedVitaminC,
                MealCount = history.MealCount,
                WaterIntake = history.WaterIntake,
                ExerciseCalories = history.ExerciseCalories,
                CreatedAt = history.CreatedAt,
                UpdatedAt = history.UpdatedAt
            };
        }

        // Meal Recommendations
        public async Task<IEnumerable<UserMealRecommendationDto>> GetUserRecommendationsAsync(int userId, bool? isViewed = null)
        {
            var query = _context.UserMealRecommendations.Where(r => r.UserId == userId);

            if (isViewed.HasValue)
                query = query.Where(r => r.IsViewed == isViewed.Value);

            var recommendations = await query
                .Select(r => new UserMealRecommendationDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    MealId = r.MealId,
                    RecommendationType = r.RecommendationType,
                    ConfidenceScore = r.ConfidenceScore,
                    Reason = r.Reason,
                    AlgorithmVersion = r.AlgorithmVersion,
                    IsViewed = r.IsViewed,
                    IsAccepted = r.IsAccepted,
                    CreatedAt = r.CreatedAt,
                    ExpiresAt = r.ExpiresAt
                })
                .ToListAsync();

            return recommendations;
        }

        public async Task<UserMealRecommendationDto> CreateRecommendationAsync(int userId, int mealId, string recommendationType, decimal confidenceScore, string? reason = null)
        {
            var recommendation = new UserMealRecommendation
            {
                UserId = userId,
                MealId = mealId,
                RecommendationType = recommendationType,
                ConfidenceScore = confidenceScore,
                Reason = reason,
                AlgorithmVersion = "v1.0",
                IsViewed = false,
                IsAccepted = null,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // Recommendations expire after 7 days
            };

            _context.UserMealRecommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            return new UserMealRecommendationDto
            {
                Id = recommendation.Id,
                UserId = recommendation.UserId,
                MealId = recommendation.MealId,
                RecommendationType = recommendation.RecommendationType,
                ConfidenceScore = recommendation.ConfidenceScore,
                Reason = recommendation.Reason,
                AlgorithmVersion = recommendation.AlgorithmVersion,
                IsViewed = recommendation.IsViewed,
                IsAccepted = recommendation.IsAccepted,
                CreatedAt = recommendation.CreatedAt,
                ExpiresAt = recommendation.ExpiresAt
            };
        }

        public async Task<UserMealRecommendationDto?> UpdateRecommendationAsync(int id, bool? isAccepted = null, bool? isViewed = null)
        {
            var recommendation = await _context.UserMealRecommendations.FindAsync(id);
            if (recommendation == null) return null;

            if (isAccepted.HasValue)
                recommendation.IsAccepted = isAccepted.Value;

            if (isViewed.HasValue)
                recommendation.IsViewed = isViewed.Value;

            await _context.SaveChangesAsync();

            return new UserMealRecommendationDto
            {
                Id = recommendation.Id,
                UserId = recommendation.UserId,
                MealId = recommendation.MealId,
                RecommendationType = recommendation.RecommendationType,
                ConfidenceScore = recommendation.ConfidenceScore,
                Reason = recommendation.Reason,
                AlgorithmVersion = recommendation.AlgorithmVersion,
                IsViewed = recommendation.IsViewed,
                IsAccepted = recommendation.IsAccepted,
                CreatedAt = recommendation.CreatedAt,
                ExpiresAt = recommendation.ExpiresAt
            };
        }

        // Meal Exclusions
        public async Task<IEnumerable<UserMealExclusionDto>> GetUserExclusionsAsync(int userId)
        {
            var exclusions = await _context.UserMealExclusions
                .Where(e => e.UserId == userId)
                .Select(e => new UserMealExclusionDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    MealId = e.MealId,
                    ExclusionType = e.ExclusionType,
                    ExclusionReason = e.ExclusionReason,
                    IsPermanent = e.IsPermanent,
                    ExcludedUntil = e.ExcludedUntil,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync();

            return exclusions;
        }

        public async Task<UserMealExclusionDto> CreateExclusionAsync(int userId, int mealId, string exclusionType, string? reason = null, bool isPermanent = true, DateTime? excludedUntil = null)
        {
            var exclusion = new UserMealExclusion
            {
                UserId = userId,
                MealId = mealId,
                ExclusionType = exclusionType,
                ExclusionReason = reason,
                IsPermanent = isPermanent,
                ExcludedUntil = excludedUntil,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserMealExclusions.Add(exclusion);
            await _context.SaveChangesAsync();

            return new UserMealExclusionDto
            {
                Id = exclusion.Id,
                UserId = exclusion.UserId,
                MealId = exclusion.MealId,
                ExclusionType = exclusion.ExclusionType,
                ExclusionReason = exclusion.ExclusionReason,
                IsPermanent = exclusion.IsPermanent,
                ExcludedUntil = exclusion.ExcludedUntil,
                CreatedAt = exclusion.CreatedAt,
                UpdatedAt = exclusion.UpdatedAt
            };
        }

        public async Task<bool> DeleteExclusionAsync(int id)
        {
            var exclusion = await _context.UserMealExclusions.FindAsync(id);
            if (exclusion == null) return false;

            _context.UserMealExclusions.Remove(exclusion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsMealExcludedAsync(int userId, int mealId)
        {
            var exclusion = await _context.UserMealExclusions
                .Where(e => e.UserId == userId && e.MealId == mealId)
                .FirstOrDefaultAsync();

            if (exclusion == null) return false;

            // Check if permanent exclusion
            if (exclusion.IsPermanent) return true;

            // Check if temporary exclusion is still valid
            if (exclusion.ExcludedUntil.HasValue && exclusion.ExcludedUntil.Value > DateTime.UtcNow)
                return true;

            return false;
        }

        // Meal Patterns
        public async Task<IEnumerable<UserMealPatternDto>> GetUserPatternsAsync(int userId)
        {
            var patterns = await _context.UserMealPatterns
                .Where(p => p.UserId == userId)
                .Select(p => new UserMealPatternDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    PatternType = p.PatternType,
                    PatternValue = p.PatternValue,
                    FrequencyCount = p.FrequencyCount,
                    ConfidenceLevel = p.ConfidenceLevel,
                    LastOccurrence = p.LastOccurrence,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return patterns;
        }

        public async Task<UserMealPatternDto> CreatePatternAsync(int userId, string patternType, string patternValue, int frequencyCount = 1, decimal confidenceLevel = 0.0m)
        {
            var pattern = new UserMealPattern
            {
                UserId = userId,
                PatternType = patternType,
                PatternValue = patternValue,
                FrequencyCount = frequencyCount,
                ConfidenceLevel = confidenceLevel,
                LastOccurrence = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserMealPatterns.Add(pattern);
            await _context.SaveChangesAsync();

            return new UserMealPatternDto
            {
                Id = pattern.Id,
                UserId = pattern.UserId,
                PatternType = pattern.PatternType,
                PatternValue = pattern.PatternValue,
                FrequencyCount = pattern.FrequencyCount,
                ConfidenceLevel = pattern.ConfidenceLevel,
                LastOccurrence = pattern.LastOccurrence,
                CreatedAt = pattern.CreatedAt,
                UpdatedAt = pattern.UpdatedAt
            };
        }

        public async Task<IEnumerable<UserMealPatternDto>> GetTopPatternsAsync(int userId, string patternType, int limit = 10)
        {
            var patterns = await _context.UserMealPatterns
                .Where(p => p.UserId == userId && p.PatternType == patternType)
                .OrderByDescending(p => p.FrequencyCount)
                .ThenByDescending(p => p.ConfidenceLevel)
                .Take(limit)
                .Select(p => new UserMealPatternDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    PatternType = p.PatternType,
                    PatternValue = p.PatternValue,
                    FrequencyCount = p.FrequencyCount,
                    ConfidenceLevel = p.ConfidenceLevel,
                    LastOccurrence = p.LastOccurrence,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return patterns;
        }

        // AI Recommendation Engine
        public async Task<IEnumerable<UserMealRecommendationDto>> GenerateRecommendationsAsync(int userId, int limit = 10)
        {
            // Get user preferences and patterns
            var preferences = await GetUserPreferencesAsync(userId);
            var patterns = await GetUserPatternsAsync(userId);
            var exclusions = await GetUserExclusionsAsync(userId);

            // Get available meals (not excluded)
            var excludedMealIds = exclusions.Select(e => e.MealId).ToList();
            var availableMeals = await _context.Meals
                .Where(m => m.StatusId == 1 && !excludedMealIds.Contains(m.Mealid))
                .Take(limit * 2) // Get more meals to filter
                .ToListAsync();

            var recommendations = new List<UserMealRecommendationDto>();

            foreach (var meal in availableMeals.Take(limit))
            {
                // Calculate confidence score based on user preferences and patterns
                var confidenceScore = CalculateConfidenceScore(userId, meal.Mealid, preferences, patterns);

                if (confidenceScore > 0.3m) // Only recommend meals with confidence > 30%
                {
                    var recommendation = await CreateRecommendationAsync(
                        userId,
                        meal.Mealid,
                        "ai_suggested",
                        confidenceScore,
                        $"Based on your preferences and eating patterns"
                    );
                    recommendations.Add(recommendation);
                }
            }

            return recommendations.OrderByDescending(r => r.ConfidenceScore).Take(limit);
        }

        public async Task UpdateUserPatternsAsync(int userId, int mealId, string action)
        {
            var meal = await _context.Meals.FindAsync(mealId);
            if (meal == null) return;

            // Update patterns based on action
            switch (action.ToLower())
            {
                case "like":
                    await CreatePatternAsync(userId, "favorite_category", meal.CategoryId.ToString(), 1, 0.8m);
                    break;
                case "dislike":
                    await CreatePatternAsync(userId, "avoid_category", meal.CategoryId.ToString(), 1, 0.8m);
                    break;
                case "cook":
                    await CreatePatternAsync(userId, "cooking_frequency", "cooked", 1, 0.6m);
                    break;
                case "skip":
                    await CreatePatternAsync(userId, "skip_frequency", "skipped", 1, 0.4m);
                    break;
            }
        }

        private decimal CalculateConfidenceScore(int userId, int mealId, IEnumerable<UserMealPreferenceDto> preferences, IEnumerable<UserMealPatternDto> patterns)
        {
            var meal = _context.Meals.Find(mealId);
            if (meal == null) return 0.0m;

            decimal score = 0.5m; // Base score

            // Check preferences
            var mealPreferences = preferences.Where(p => p.MealId == mealId);
            foreach (var pref in mealPreferences)
            {
                score += pref.PreferenceScore * 0.3m;
            }

            // Check patterns
            var categoryPatterns = patterns.Where(p => p.PatternType == "favorite_category" && p.PatternValue == meal.CategoryId.ToString());
            foreach (var pattern in categoryPatterns)
            {
                score += pattern.ConfidenceLevel * 0.2m;
            }

            // Normalize score to 0-1 range
            return Math.Max(0.0m, Math.Min(1.0m, score));
        }
    }
}
