using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IAIIngredientRepo
    {
        Task<List<Ingredient>> GetAllIngredientsAsync(bool onlyActive = true);
        Task<Ingredient?> GetByIdAsync(int id);
        Task<List<Meal>> GetDrinksFromMealsAsync();
        Task<List<Meal>> GetMealsAsync();
        Task<List<Meal>> GetMealsForMealtimeAsync(
            string mealtimeName,
            List<int> avoidIngredientIds,
            List<int> preferredIngredientIds
        );
    }
}
