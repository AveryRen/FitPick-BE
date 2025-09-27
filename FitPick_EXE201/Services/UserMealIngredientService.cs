using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class UserMealIngredientService
    {
        private readonly IUserMealIngredientRepo _repo;

        public UserMealIngredientService(IUserMealIngredientRepo repo)
        {
            _repo = repo;
        }

        public async Task<List<UserMealIngredientDto>> GetUserMealIngredientsAsync(int userId, int mealId)
        {
            var repoResult = await _repo.GetUserMealIngredientsAsync(userId, mealId);

            return repoResult.Select(x => new UserMealIngredientDto
            {
                IngredientId = x.IngredientId,   
                IngredientName = x.Name,
                Quantity = x.Quantity,
                Unit = x.Unit,
                HasIt = x.HasIt
            }).ToList();
        }

        public Task MarkIngredientAsync(int userId, int mealId, int ingredientId, bool hasIt)
        {
            return _repo.MarkIngredientAsync(userId, mealId, ingredientId, hasIt);
        }
    }
}
