using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IRecommendationRepo
    {
        Task<List<Meal>> GetMealsByUserGoalAsync(int userId, int count);
        Task<List<Meal>> GetRandomMealsAsync(int count);
        Task<List<Ingredient>> GetIngredientRecommendationsAsync(int mealId, int count);
    }
}
