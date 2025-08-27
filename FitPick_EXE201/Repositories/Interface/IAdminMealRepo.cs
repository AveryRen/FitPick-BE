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

        Task<Meal?> GetByIdAsync(int id);
        Task<Meal> AddAsync(Meal meal);
        Task<Meal> UpdateAsync(Meal meal);
        Task<bool> DeleteAsync(int id);
    }
}
