using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class UserIngredientRepo : IUserIngredientRepo
    {
        private readonly FitPickContext _context;

        public UserIngredientRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<UserIngredient> AddAsync(UserIngredient entity)
        {
            await _context.UserIngredients.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<UserIngredient>> GetByUserIdAsync(int userId)
        {
            var items = await _context.UserIngredients
                .Include(ui => ui.Ingredient)
                .Where(ui => ui.Userid == userId)
                .ToListAsync();

            // map unit
            foreach (var ui in items)
            {
                if (ui.Ingredient != null)
                    ui.Unit = ui.Ingredient.Unit;
            }

            return items;
        }

        public async Task<UserIngredient?> GetByIdAsync(int id)
        {
            var ui = await _context.UserIngredients
                .Include(ui => ui.Ingredient)
                .FirstOrDefaultAsync(ui => ui.Id == id);

            if (ui?.Ingredient != null)
                ui.Unit = ui.Ingredient.Unit;

            return ui;
        }


        public async Task<UserIngredient?> UpdateAsync(UserIngredient entity)
        {
            var existing = await _context.UserIngredients.FindAsync(entity.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(entity);
            existing.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> ResetQuantityAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            entity.Quantity = 0;
            entity.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.UserIngredients.FindAsync(id);
            if (entity == null) return false;

            _context.UserIngredients.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<UserIngredient?> GetByUserIdAndIngredientIdAsync(int userId, int ingredientId)
        {
            return await _context.UserIngredients
                .FirstOrDefaultAsync(ui => ui.Userid == userId && ui.Ingredientid == ingredientId);
        }
    }
}
