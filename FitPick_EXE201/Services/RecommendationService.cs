using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class RecommendationService
    {
        private readonly IRecommendationRepo _repo;

        public RecommendationService(IRecommendationRepo repo)
        {
            _repo = repo;
        }

        public async Task<List<MealRecommendationDto>> GetMealRecommendationsAsync(int userId, int count = 5)
        {
            var meals = await _repo.GetMealsByUserGoalAsync(userId, count);

            if (meals == null || meals.Count == 0)
                meals = await _repo.GetRandomMealsAsync(count);

            return meals.Select(m => new MealRecommendationDto
            {
                Mealid = m.Mealid,
                Name = m.Name,
                Calories = (int)(m.Calories ?? 0),
                Protein = (int)(m.Protein ?? 0),
                Carbs = (int)(m.Carbs ?? 0),
                Fat = (int)(m.Fat ?? 0),
                IsPremium = m.IsPremium ?? false
            }).ToList();
        }

        public async Task<List<IngredientRecommendationDto>> GetIngredientRecommendationsAsync(int mealId, int count = 5)
        {
            var ingredients = await _repo.GetIngredientRecommendationsAsync(mealId, count);

            return ingredients.Select(i => new IngredientRecommendationDto
            {
                Ingredientid = i.Ingredientid,
                Name = i.Name,
                IsHealthyAlternative = true
            }).ToList();
        }
    }
}
