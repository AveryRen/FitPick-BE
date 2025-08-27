using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class UserMealService
    {
        private readonly IUserMealRepository _userMealRepository;

        public UserMealService(IUserMealRepository userMealRepository)
        {
            _userMealRepository = userMealRepository;
        }

         public async Task<IEnumerable<Meal>> GetMealsAsync(
            string? name = null,
            int? categoryId = null,
            string? dietType = null,
            int? minCalories = null,
            int? maxCalories = null,
            int? minCookingTime = null,
            int? maxCookingTime = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? statusId = null)
        {
            return await _userMealRepository.GetMealsAsync(
                name, categoryId, dietType,
                minCalories, maxCalories,
                minCookingTime, maxCookingTime,
                minPrice, maxPrice, statusId
            );
        }

         public async Task<Meal?> GetMealByIdAsync(int id)
        {
            return await _userMealRepository.GetMealByIdAsync(id);
        }
    }
} 
