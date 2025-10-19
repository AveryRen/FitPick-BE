using Microsoft.EntityFrameworkCore;
using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Repositories.Repo
{
    public class MealDetailRepository : IMealDetailRepository
    {
        private readonly FitPickContext _context;

        public MealDetailRepository(FitPickContext context)
        {
            _context = context;
        }

        public async Task<MealDetailDto?> GetMealDetailByIdAsync(int mealId)
        {
            var meal = await _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Status)
                .FirstOrDefaultAsync(m => m.Mealid == mealId);

            if (meal == null) return null;

            return new MealDetailDto
            {
                Mealid = meal.Mealid,
                Name = meal.Name,
                Description = meal.Description,
                Calories = meal.Calories,
                Protein = meal.Protein,
                Carbs = meal.Carbs,
                Fat = meal.Fat,
                Cookingtime = meal.Cookingtime,
                Diettype = meal.Diettype,
                Price = meal.Price,
                ImageUrl = meal.ImageUrl,
                IsPremium = meal.IsPremium,
                CategoryName = meal.Category?.Name,
                StatusName = meal.Status?.Name
            };
        }

        public async Task<List<MealIngredientDetailDto>> GetMealIngredientsAsync(int mealId)
        {
            var ingredients = await _context.Mealingredients
                .Where(mi => mi.Mealid == mealId)
                .Include(mi => mi.Ingredient)
                .Select(mi => new MealIngredientDetailDto
                {
                    IngredientId = mi.Ingredientid,
                    IngredientName = mi.Ingredient.Name,
                    IngredientType = mi.Ingredient.Type,
                    Quantity = mi.Quantity,
                    Unit = mi.Ingredient.Unit
                })
                .OrderBy(mi => mi.IngredientName)
                .ToListAsync();

            return ingredients;
        }

        public async Task<List<MealInstructionDto>> GetMealInstructionsAsync(int mealId)
        {
            var instructions = await _context.MealInstructions
                .Where(mi => mi.MealId == mealId)
                .Select(mi => new MealInstructionDto
                {
                    MealId = mi.MealId,
                    StepNumber = mi.StepNumber,
                    Instruction = mi.Instruction
                })
                .OrderBy(mi => mi.StepNumber)
                .ToListAsync();

            return instructions;
        }
    }
}
