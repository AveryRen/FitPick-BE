using FitPick_EXE201.Data;
using FitPick_EXE201.Models;
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
            string? name = null,
            int? categoryId = null,
            string? dietType = null,
            int? minCalories = null,
            int? maxCalories = null,
            int? minCookingTime = null,
            int? maxCookingTime = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? statusId = null)
        {
            var query = _context.Meals.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(m => m.Name.Contains(name));

            if (categoryId.HasValue)
                query = query.Where(m => m.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(dietType))
                query = query.Where(m => m.Diettype == dietType);

            if (minCalories.HasValue)
                query = query.Where(m => m.Calories >= minCalories.Value);

            if (maxCalories.HasValue)
                query = query.Where(m => m.Calories <= maxCalories.Value);

            if (minCookingTime.HasValue)
                query = query.Where(m => m.Cookingtime >= minCookingTime.Value);

            if (maxCookingTime.HasValue)
                query = query.Where(m => m.Cookingtime <= maxCookingTime.Value);
            
            if (minPrice.HasValue)
                query = query.Where(m => m.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(m => m.Price <= maxPrice.Value);

            if (statusId.HasValue)
                query = query.Where(m => m.StatusId == statusId.Value);

            return await query.ToListAsync();
        }

         // GET /api/meals/{id}
        public async Task<Meal?> GetMealByIdAsync(int id)
        {
            return await _context.Meals
                .Include(m => m.Category)   // nếu cần load category
                .Include(m => m.Status)     // nếu cần load status
                .FirstOrDefaultAsync(m => m.Mealid == id);
        }
    }
}
