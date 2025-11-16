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

            // filter theo t�n (search contains)
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(i => i.Name.Contains(name));

            // filter theo type (exact match)
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(i => i.Type == type);

            // filter theo don v?
            if (!string.IsNullOrWhiteSpace(unit))
                query = query.Where(i => i.Unit == unit);

            return await query
                .OrderByDescending(i => i.Ingredientid)
                .ToListAsync();
        }

        public async Task<(List<Ingredient> items, int totalCount)> GetAllPagedAsync(
            int page,
            int pageSize,
            string? name = null,
            string? type = null,
            string? unit = null,
            bool? status = null,
            string? sortBy = "ingredientid",
            bool sortDesc = true
        )
        {
            var query = _context.Set<Ingredient>().AsQueryable();

            // Filter status
            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            // Filter by name (search contains)
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(i => i.Name.Contains(name));

            // Filter by type (exact match)
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(i => i.Type == type);

            // Filter by unit
            if (!string.IsNullOrWhiteSpace(unit))
                query = query.Where(i => i.Unit == unit);

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "name" => sortDesc ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name),
                "type" => sortDesc ? query.OrderByDescending(i => i.Type ?? "") : query.OrderBy(i => i.Type ?? ""),
                "ingredientid" => sortDesc ? query.OrderByDescending(i => i.Ingredientid) : query.OrderBy(i => i.Ingredientid),
                _ => query.OrderByDescending(i => i.Ingredientid)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Ingredient?> GetByIdAsync(int id)
        {
            return await _context.Set<Ingredient>().FindAsync(id);
        }

        public async Task<Ingredient> AddAsync(Ingredient ingredient)
        {
            // Ensure Ingredientid is not set (let database generate it)
            ingredient.Ingredientid = 0;
            
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

            // Actually delete from database instead of just setting inactive
            _context.Set<Ingredient>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
