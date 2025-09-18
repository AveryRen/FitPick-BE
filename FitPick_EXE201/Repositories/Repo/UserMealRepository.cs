using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class UserMealRepository : IUserMealRepository
    {
        private readonly FitPickContext _context;

        public UserMealRepository(FitPickContext context)
        {
            _context = context;
        }

        // GET /api/meals (có filter)
        public async Task<IEnumerable<Meal>> GetMealsAsync(
            string? name,
            int? categoryId,
            string? dietType,
            int? minCalories,
            int? maxCalories,
            int? minCookingTime,
            int? maxCookingTime,
            decimal? minPrice,
            decimal? maxPrice,
            int? statusId
        )
        {
            var query = _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Status)
                .Include(m => m.MealInstructions)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(name))
                query = query.Where(m => m.Name.Contains(name));

            if (categoryId.HasValue)
                query = query.Where(m => m.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(dietType))
                query = query.Where(m => m.Diettype == dietType);

            if (minCalories.HasValue)
                query = query.Where(m => m.Calories >= minCalories);

            if (maxCalories.HasValue)
                query = query.Where(m => m.Calories <= maxCalories);

            if (minCookingTime.HasValue)
                query = query.Where(m => m.Cookingtime >= minCookingTime);

            if (maxCookingTime.HasValue)
                query = query.Where(m => m.Cookingtime <= maxCookingTime);

            if (minPrice.HasValue)
                query = query.Where(m => m.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(m => m.Price <= maxPrice);

            if (statusId.HasValue)
                query = query.Where(m => m.StatusId == statusId);

            return await query.ToListAsync();
        }

        // GET /api/meals/{id} (full include)
        public async Task<Meal?> GetMealByIdAsync(int id)
        {
            return await _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Status)
                .Include(m => m.MealInstructions)
                .Include(m => m.Mealingredients)
                    .ThenInclude(mi => mi.Ingredient)
                .Include(m => m.MealHistories)
                .FirstOrDefaultAsync(m => m.Mealid == id);
        }
    }
}
