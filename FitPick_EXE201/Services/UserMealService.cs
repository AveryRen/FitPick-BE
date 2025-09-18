using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class UserMealService
    {
        private readonly IUserMealRepository _mealRepository;

        public UserMealService(IUserMealRepository mealRepository)
        {
            _mealRepository = mealRepository;
        }

        public async Task<IEnumerable<MealDto>> GetMealsAsync(
            string? name, int? categoryId, string? dietType,
            int? minCalories, int? maxCalories,
            int? minCookingTime, int? maxCookingTime,
            decimal? minPrice, decimal? maxPrice, int userId)
        {
            var meals = await _mealRepository.GetMealsAsync(
                name, categoryId, dietType,
                minCalories, maxCalories,
                minCookingTime, maxCookingTime,
                minPrice, maxPrice, statusId: 1 // chỉ lấy Published
            );

            return meals.Select(m => new MealDto
            {
                Mealid = m.Mealid,
                Name = m.Name,
                Description = m.Description,
                Calories = m.Calories,
                Cookingtime = m.Cookingtime,
                Diettype = m.Diettype,
                Price = m.Price,
                ImageUrl = m.ImageUrl,
                IsPremium = m.IsPremium,
                CategoryName = m.Category?.Name,
                StatusName = m.Status?.Name,
                Instructions = m.MealInstructions?
                                .OrderBy(i => i.StepNumber)
                                .Select(i => i.Instruction)
                                .ToList()
            });
        }

        public async Task<MealDto?> GetMealByIdAsync(int id)
        {
            var meal = await _mealRepository.GetMealByIdAsync(id);
            if (meal == null) return null;

            return new MealDto
            {
                Mealid = meal.Mealid,
                Name = meal.Name,
                Description = meal.Description,
                Calories = meal.Calories,
                Cookingtime = meal.Cookingtime,
                Diettype = meal.Diettype,
                Price = meal.Price,
                ImageUrl = meal.ImageUrl,
                IsPremium = meal.IsPremium,
                CategoryName = meal.Category?.Name,
                StatusName = meal.Status?.Name,
                Instructions = meal.MealInstructions?
                                .OrderBy(i => i.StepNumber)
                                .Select(i => i.Instruction)
                                .ToList()
            };
        }
    }
}
