using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IUserIngredientRepo
    {
        Task<UserIngredient> AddAsync(UserIngredient entity);
        Task<IEnumerable<UserIngredient>> GetByUserIdAsync(int userId);
        Task<UserIngredient?> GetByIdAsync(int id);
        Task<UserIngredient?> UpdateAsync(UserIngredient entity);
        Task<bool> ResetQuantityAsync(int id);
        Task<UserIngredient?> GetByUserIdAndIngredientIdAsync(int userId, int ingredientId);

    }
}
