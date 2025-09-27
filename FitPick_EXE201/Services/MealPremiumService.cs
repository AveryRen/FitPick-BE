using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class MealPremiumService
    {
        private readonly IUserMealPremiumRepo _repo;

        public MealPremiumService(IUserMealPremiumRepo repo)
        {
            _repo = repo;
        }

        // Lấy tất cả món premium
        public async Task<List<MealResponseDto>> GetPremiumMealsAsync()
        {
            var meals = await _repo.GetPremiumMealsAsync();
            return meals.Select(m => new MealResponseDto
            {
                Mealid = m.Mealid,
                Name = m.Name,
                Description = m.Description,
                Calories = m.Calories,
                Cookingtime = m.Cookingtime,
                ImageUrl = m.ImageUrl,
                Protein = m.Protein,
                Carbs = m.Carbs,
                Fat = m.Fat,
                Price = m.Price
            }).ToList();
        }

        // Filter món premium
        public async Task<List<MealResponseDto>> FilterPremiumMealsAsync(MealFilterDto filter)
        {
            var query = _repo.GetPremiumMealsQuery();

            if (filter.CategoryId.HasValue)
                query = query.Where(m => m.CategoryId == filter.CategoryId.Value);

            if (filter.MaxCalories.HasValue)
                query = query.Where(m => m.Calories <= filter.MaxCalories.Value);

            if (!string.IsNullOrEmpty(filter.DietType))
                query = query.Where(m => m.Diettype == filter.DietType);

            var meals = await query.ToListAsync();

            return meals.Select(m => new MealResponseDto
            {
                Mealid = m.Mealid,
                Name = m.Name,
                Description = m.Description,
                Calories = m.Calories,
                Cookingtime = m.Cookingtime,
                ImageUrl = m.ImageUrl,
                Protein = m.Protein,
                Carbs = m.Carbs,
                Fat = m.Fat,
                Price = m.Price
            }).ToList();
        }
    }
}
