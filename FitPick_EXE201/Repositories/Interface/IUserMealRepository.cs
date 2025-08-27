using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IUserMealRepository
    {
        Task<IEnumerable<Meal>> GetMealsAsync(
            string? name = null,
            int? categoryId = null,
            string? dietType = null,
            int? minCalories = null,
            int? maxCalories = null,
            int? minCookingTime = null,
            int? maxCookingTime = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? statusId = null);

        Task<Meal?> GetMealByIdAsync(int id);
    }
}
