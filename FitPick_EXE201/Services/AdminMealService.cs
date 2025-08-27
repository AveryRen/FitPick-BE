using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class AdminMealService
    {
        private readonly IAdminMealRepo _mealRepo;

        public AdminMealService(IAdminMealRepo mealRepo)
        {
            _mealRepo = mealRepo;
        }

        public async Task<IEnumerable<Meal>> GetAllAsync(
            int? categoryId = null,
            int? minCalories = null,
            int? maxCalories = null,
            decimal? minPrice = null,
            decimal? maxPrice = null
        )
        {
            return await _mealRepo.GetAllAsync(categoryId, minCalories, maxCalories, minPrice, maxPrice);
        }

        public async Task<Meal?> GetByIdAsync(int id)
        {
            return await _mealRepo.GetByIdAsync(id);
        }

        public async Task<Meal> AddAsync(Meal meal)
        {
            return await _mealRepo.AddAsync(meal);
        }

        public async Task<Meal> UpdateAsync(Meal meal)
        {
            return await _mealRepo.UpdateAsync(meal);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _mealRepo.DeleteAsync(id);
        }
    }
} 
