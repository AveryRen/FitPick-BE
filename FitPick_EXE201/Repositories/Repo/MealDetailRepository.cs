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
            try
            {
                Console.WriteLine($"=== GetMealInstructionsAsync called for mealId: {mealId} ===");
                
                // First, check if there are any instructions in the table at all
                var totalInstructions = await _context.MealInstructions.CountAsync();
                Console.WriteLine($"Total instructions in database: {totalInstructions}");
                
                // Check instructions for this specific meal
                var instructionsForMeal = await _context.MealInstructions
                    .Where(mi => mi.MealId == mealId)
                    .CountAsync();
                Console.WriteLine($"Instructions with MealId = {mealId}: {instructionsForMeal}");
                
                // Get all instructions (for debugging)
                var allInstructions = await _context.MealInstructions
                    .Take(10)
                    .Select(mi => new { mi.MealId, mi.StepNumber, mi.Instruction })
                    .ToListAsync();
                Console.WriteLine($"Sample instructions (first 10):");
                foreach (var inst in allInstructions)
                {
                    Console.WriteLine($"  - MealId: {inst.MealId}, Step: {inst.StepNumber}, Instruction: {inst.Instruction?.Substring(0, Math.Min(50, inst.Instruction?.Length ?? 0))}...");
                }
                
                // Now get the actual instructions for this meal
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

                Console.WriteLine($"GetMealInstructionsAsync: Found {instructions.Count} instructions for meal {mealId}");
                if (instructions.Count > 0)
                {
                    foreach (var inst in instructions)
                    {
                        Console.WriteLine($"  - Step {inst.StepNumber}: {inst.Instruction}");
                    }
                }
                else
                {
                    Console.WriteLine($"WARNING: No instructions found for meal {mealId}. Checking if meal exists...");
                    var mealExists = await _context.Meals.AnyAsync(m => m.Mealid == mealId);
                    Console.WriteLine($"Meal {mealId} exists: {mealExists}");
                }
                
                return instructions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetMealInstructionsAsync for meal {mealId}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}
