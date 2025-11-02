using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IAdminMealRepo
    {
        Task<IEnumerable<Meal>> GetAllAsync(
                    int? categoryId = null,
                    int? minCalories = null,
                    int? maxCalories = null,
                    decimal? minPrice = null,
                    decimal? maxPrice = null
                );

        Task<(List<Meal> items, int totalCount)> GetAllPagedAsync(
                    int page,
                    int pageSize,
                    int? categoryId = null,
                    string? dietType = null,
                    int? statusId = null,
                    string? search = null,
                    string? sortBy = "createdat",
                    bool sortDesc = true
                );

        Task<Meal?> GetByIdAsync(int id);
        Task<Meal> AddAsync(Meal meal);
        Task<Meal> UpdateAsync(Meal meal);
        Task<bool> DeleteAsync(int id);
        Task<Meal?> UpdateImageAsync(int id, string imageUrl);
        Task AddIngredientsAsync(int mealId, List<Models.DTOs.MealIngredientCreateDto> ingredients);
        Task RemoveIngredientsAsync(int mealId);
    }
}
