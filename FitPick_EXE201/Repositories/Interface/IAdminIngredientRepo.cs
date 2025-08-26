using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IAdminIngredientRepo
    {
        Task<IEnumerable<Ingredient>> GetAllAsync(string? name = null,string? type = null,
            string? unit = null,
            bool onlyActive = true);
        Task<Ingredient?> GetByIdAsync(int id);
        Task<Ingredient> AddAsync(Ingredient ingredient);
        Task<Ingredient> UpdateAsync(Ingredient ingredient);
        Task<bool> DeleteAsync(int id);
    }
}
