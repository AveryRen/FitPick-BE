using FitPick_EXE201.Models.DTOs;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IMealDetailRepository
    {
        Task<MealDetailDto?> GetMealDetailByIdAsync(int mealId);
        Task<List<MealIngredientDetailDto>> GetMealIngredientsAsync(int mealId);
        Task<List<MealInstructionDto>> GetMealInstructionsAsync(int mealId);
    }
}
