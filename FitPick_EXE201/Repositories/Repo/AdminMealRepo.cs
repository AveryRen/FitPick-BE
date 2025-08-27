using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class AdminMealRepo : IAdminMealRepo
    {
        private readonly FitPickContext _context;

        public AdminMealRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Meal>> GetAllAsync(
            int? categoryId = null,
            int? minCalories = null,
            int? maxCalories = null,
            decimal? minPrice = null,
            decimal? maxPrice = null
        )
        {
            var query = _context.Meals.AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(m => m.CategoryId == categoryId.Value);

            if (minCalories.HasValue)
                query = query.Where(m => m.Calories >= minCalories.Value);

            if (maxCalories.HasValue)
                query = query.Where(m => m.Calories <= maxCalories.Value);

            if (minPrice.HasValue)
                query = query.Where(m => m.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(m => m.Price <= maxPrice.Value);

            return await query
                .Include(m => m.Category)   // nếu Meal có navigation Category
                .ToListAsync();
        }
        public async Task<Meal?> GetByIdAsync(int id)
        {
            return await _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Mealingredients)
                    .ThenInclude(mi => mi.Ingredient)
                .FirstOrDefaultAsync(m => m.Mealid == id);
        }

        public async Task<Meal> AddAsync(Meal meal)
        {
            _context.Meals.Add(meal);
            await _context.SaveChangesAsync();
            return meal;
        }

        public async Task<Meal> UpdateAsync(Meal meal)
        {
            _context.Meals.Update(meal);
            await _context.SaveChangesAsync();
            return meal;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var meal = await _context.Meals.FindAsync(id);
            if (meal == null) return false;

            _context.Meals.Remove(meal);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 
