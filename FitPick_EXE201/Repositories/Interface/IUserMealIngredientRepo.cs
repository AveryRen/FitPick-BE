using FitPick_EXE201.Models.DTOs;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IUserMealIngredientRepo
    {
        Task<List<MealIngredientDto>> GetUserMealIngredientsAsync(int userId, int mealId);
        Task MarkIngredientAsync(int userId, int mealId, int ingredientId, bool hasIt);
    }
}
