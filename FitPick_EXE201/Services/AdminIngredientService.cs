using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class AdminIngredientService
    {
        private readonly IAdminIngredientRepo _repository;

        public AdminIngredientService(IAdminIngredientRepo repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Ingredient>> GetAllAsync(
            string? name = null,
            string? type = null,
            string? unit = null,
            bool onlyActive = true)
        {
            return await _repository.GetAllAsync(name, type, unit, onlyActive);
        }

        public async Task<Ingredient?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Ingredient> CreateAsync(IngredientCreateDto dto)
        {
            var ingredient = new Ingredient
            {
                Name = dto.Name,
                Type = dto.Type,
                Unit = dto.Unit,
                Status = true 
            };

            await _repository.AddAsync(ingredient);
            return ingredient;
        }

        public async Task<Ingredient?> UpdateAsync(int id, IngredientUpdateDto dto)
        {
            var ingredient = await _repository.GetByIdAsync(id);
            if (ingredient == null) return null;

            ingredient.Name = dto.Name;
            ingredient.Type = dto.Type;
            ingredient.Unit = dto.Unit;
            ingredient.Status = dto.Status;

            await _repository.UpdateAsync(ingredient);
            return ingredient;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
