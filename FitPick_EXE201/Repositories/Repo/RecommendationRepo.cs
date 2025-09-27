using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class RecommendationRepo : IRecommendationRepo
    {
        private readonly FitPickContext _context;

        public RecommendationRepo(FitPickContext context)
        {
            _context = context;
        }

        // Lấy meals theo user goal (qua Healthprofile vì Healthgoal không có UserId)
        public async Task<List<Meal>> GetMealsByUserGoalAsync(int userId, int count)
        {
            var profile = await _context.Healthprofiles
                .Include(hp => hp.Healthgoal)
                .FirstOrDefaultAsync(hp => hp.Userid == userId);

            // Lấy target calories từ Healthprofile
            int targetCalories = profile?.Targetcalories ?? 0;

            if (targetCalories == 0)
                return await GetRandomMealsAsync(count);

            return await _context.Meals
                .Where(m => (m.Calories ?? 0) <= targetCalories) // fix nullable
                .OrderBy(r => Guid.NewGuid()) // random
                .Take(count)
                .ToListAsync();
        }

        // Lấy meals ngẫu nhiên
        public async Task<List<Meal>> GetRandomMealsAsync(int count)
        {
            return await _context.Meals
                .OrderBy(r => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }

        // Lấy gợi ý ingredient
        public async Task<List<Ingredient>> GetIngredientRecommendationsAsync(int mealId, int count)
        {
            var currentIngredientIds = await _context.Mealingredients
                .Where(mi => mi.Mealid == mealId)
                .Select(mi => mi.Ingredientid)
                .ToListAsync();

            return await _context.Ingredients
                .Where(i => !currentIngredientIds.Contains(i.Ingredientid)) // fix Id -> Ingredientid, bỏ IsHealthy
                .OrderBy(r => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }
    }
}
