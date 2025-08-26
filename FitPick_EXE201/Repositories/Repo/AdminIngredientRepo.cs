using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class AdminIngredientRepo : IAdminIngredientRepo
    {
        private readonly FitPickContext _context;

        public AdminIngredientRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ingredient>> GetAllAsync(
            string? name = null,
            string? type = null,
            string? unit = null,
            bool onlyActive = true)
        {
            var query = _context.Set<Ingredient>().AsQueryable();

            // filter status
            if (onlyActive)
                query = query.Where(i => i.Status == true);

            // filter theo tên (search contains)
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(i => i.Name.Contains(name));

            // filter theo type (exact match)
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(i => i.Type == type);

            // filter theo đơn vị
            if (!string.IsNullOrWhiteSpace(unit))
                query = query.Where(i => i.Unit == unit);

            return await query.ToListAsync();
        }


        public async Task<Ingredient?> GetByIdAsync(int id)
        {
            return await _context.Set<Ingredient>().FindAsync(id);
        }

        public async Task<Ingredient> AddAsync(Ingredient ingredient)
        {
            _context.Set<Ingredient>().Add(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<Ingredient> UpdateAsync(Ingredient ingredient)
        {
            _context.Set<Ingredient>().Update(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = false;

            _context.Set<Ingredient>().Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
