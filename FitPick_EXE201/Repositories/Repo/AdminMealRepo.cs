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
                .Include(m => m.Category)
                .Include(m => m.MealInstructions)
                .Include(m => m.Status)
                .OrderByDescending(m => m.Createdat ?? DateTime.MinValue)
                .ToListAsync();
        }

        public async Task<(List<Meal> items, int totalCount)> GetAllPagedAsync(
            int page,
            int pageSize,
            int? categoryId = null,
            string? dietType = null,
            int? statusId = null,
            string? search = null,
            string? sortBy = "createdat",
            bool sortDesc = true
        )
        {
            var query = _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Status)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(m => m.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(dietType))
                query = query.Where(m => m.Diettype == dietType);

            if (statusId.HasValue)
                query = query.Where(m => m.StatusId == statusId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => 
                    m.Name.Contains(search) ||
                    (m.Description != null && m.Description.Contains(search))
                );
            }

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "name" => sortDesc ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name),
                "calories" => sortDesc ? query.OrderByDescending(m => m.Calories ?? 0) : query.OrderBy(m => m.Calories ?? 0),
                "price" => sortDesc ? query.OrderByDescending(m => m.Price ?? 0) : query.OrderBy(m => m.Price ?? 0),
                "createdat" => sortDesc ? query.OrderByDescending(m => m.Createdat ?? DateTime.MinValue) : query.OrderBy(m => m.Createdat ?? DateTime.MinValue),
                _ => query.OrderByDescending(m => m.Createdat ?? DateTime.MinValue)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task<Meal?> GetByIdAsync(int id)
        {
            return await _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Mealingredients)
                    .ThenInclude(mi => mi.Ingredient)
                .Include(m => m.MealInstructions)
                .Include(m => m.Status)
                .FirstOrDefaultAsync(m => m.Mealid == id);
        }


        public async Task<Meal> AddAsync(Meal meal)
        {
            // Ensure Mealid is not set (let database generate it)
            meal.Mealid = 0;
            
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
        public async Task<Meal?> UpdateImageAsync(int id, string imageUrl)
        {
            var meal = await _context.Meals.FindAsync(id);
            if (meal == null) return null;

            meal.ImageUrl = imageUrl;
            await _context.SaveChangesAsync();

            return meal;
        }

        public async Task AddIngredientsAsync(int mealId, List<Models.DTOs.MealIngredientCreateDto> ingredients)
        {
            var mealIngredients = ingredients.Select(i => new Mealingredient
            {
                Mealid = mealId,
                Ingredientid = i.IngredientId,
                Quantity = i.Quantity
            }).ToList();

            _context.Mealingredients.AddRange(mealIngredients);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveIngredientsAsync(int mealId)
        {
            var existingIngredients = await _context.Mealingredients
                .Where(mi => mi.Mealid == mealId)
                .ToListAsync();

            _context.Mealingredients.RemoveRange(existingIngredients);
            await _context.SaveChangesAsync();
        }
    }
} 
