using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class UserMealPremiumRepo : IUserMealPremiumRepo
    {
        private readonly FitPickContext _context;

        public UserMealPremiumRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<List<Meal>> GetPremiumMealsAsync()
        {
            return await _context.Meals
                .Where(m => m.IsPremium == true)
                .ToListAsync();
        }

        public IQueryable<Meal> GetPremiumMealsQuery()
        {
            return _context.Meals
                .Where(m => m.IsPremium == true)
                .AsQueryable();
        }
    }
}