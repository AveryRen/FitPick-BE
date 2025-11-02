using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.DTOs;
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

        public async Task<(List<Meal> items, int totalCount)> GetAllPagedAsync(
            int page,
            int pageSize,
            int? categoryId = null,
            string? dietType = null,
            int? statusId = null,
            string? search = null,
            string? sortBy = "createdat",
            bool sortDesc = true
        )
        {
            return await _mealRepo.GetAllPagedAsync(page, pageSize, categoryId, dietType, statusId, search, sortBy, sortDesc);
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
        public async Task<Meal?> UpdateImageAsync(int id, string imageUrl)
        {
            return await _mealRepo.UpdateImageAsync(id, imageUrl);
        }

        public async Task AddIngredientsAsync(int mealId, List<MealIngredientCreateDto> ingredients)
        {
            await _mealRepo.AddIngredientsAsync(mealId, ingredients);
        }

        public async Task RemoveIngredientsAsync(int mealId)
        {
            await _mealRepo.RemoveIngredientsAsync(mealId);
        }
    }
} 
