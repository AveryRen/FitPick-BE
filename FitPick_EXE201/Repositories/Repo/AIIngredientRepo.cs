using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FitPick_EXE201.Repositories.Repo
{
    public class AIIngredientRepo : IAIIngredientRepo
    {
        private readonly FitPickContext _context;

        public AIIngredientRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<List<Ingredient>> GetAllIngredientsAsync(bool onlyActive = true)
        {
            var query = _context.Set<Ingredient>().AsQueryable();

            if (onlyActive)
                query = query.Where(i => i.Status == true);

            return await query.ToListAsync();
        }

        public async Task<Ingredient?> GetByIdAsync(int id)
        {
            return await _context.Set<Ingredient>().FindAsync(id);
        }
        public async Task<List<Meal>> GetDrinksFromMealsAsync()
        {
            return await _context.Meals
                .Where(m => m.CategoryId == 4) // category_id = 4 là nước uống
                .ToListAsync();
        }
        public async Task<List<Meal>> GetMealsAsync()
        {
            return await _context.Meals
                .Where(m => m.CategoryId != 4)
                .ToListAsync();
        }
        public async Task<List<Meal>> GetMealsForMealtimeAsync(
            string mealtimeName,
            List<int> avoidIngredientIds,
            List<int> preferredIngredientIds
        )
        {
            // Lấy mealtime
            var mealtime = await _context.MealTimes
                .FirstOrDefaultAsync(m => m.Name.ToLower() == mealtimeName.ToLower());

            if (mealtime == null) return new List<Meal>();

            // Lấy tất cả meals theo mealtime (trong DB chưa có mapping mealtime trực tiếp? Nếu mealplans thì có)
            var mealsQuery = _context.Meals.AsQueryable();

            // Loại bỏ món có nguyên liệu trong avoidIngredientIds
            if (avoidIngredientIds != null && avoidIngredientIds.Count > 0)
            {
                mealsQuery = mealsQuery.Where(m =>
                    !m.Mealingredients.Any(mi => mi.Ingredientid.HasValue && avoidIngredientIds.Contains(mi.Ingredientid.Value))
                );
            }

            // Ưu tiên món có nguyên liệu trong preferredIngredientIds
            if (preferredIngredientIds != null && preferredIngredientIds.Count > 0)
            {
                mealsQuery = mealsQuery.OrderByDescending(m =>
                    m.Mealingredients.Any(mi => mi.Ingredientid.HasValue && avoidIngredientIds.Contains(mi.Ingredientid.Value))
                );
            }

            return await mealsQuery.ToListAsync();
        }
    }
}
