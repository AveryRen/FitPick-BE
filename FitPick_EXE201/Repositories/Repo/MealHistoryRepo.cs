using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace FitPick_EXE201.Repositories.Repo
{
    public class MealHistoryRepo : IMealHistoryRepo
    {
        private readonly FitPickContext _context;

        public MealHistoryRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MealHistory>> GetUserHistoryAsync(int userId)
        {
            return await _context.MealHistories
                .Where(h => h.Userid == userId)
                .Include(h => h.Meal)
                .Include(h => h.Mealtime)
                .Include(h => h.User)
                .OrderByDescending(h => h.Date)
                .ToListAsync();
        }


        public async Task<MealHistory?> GetByIdAsync(int id)
        {
            return await _context.MealHistories
                .Include(m => m.Meal)
                .Include(m => m.Mealtime)
                .FirstOrDefaultAsync(h => h.Historyid == id);
        }

        public async Task AddAsync(MealHistory history)
        {
            await _context.MealHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MealHistory history)
        {
            _context.MealHistories.Remove(history);
            await _context.SaveChangesAsync();
        }

        public async Task<object> GetDailyStatsAsync(int userId, DateOnly date)
        {
            var stats = await _context.MealHistories
                .Where(h => h.Userid == userId && h.Date == date)
                .GroupBy(h => h.Userid)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalCalories = g.Sum(x => x.Calories ?? 0),
                    TotalMeals = g.Count()
                })
                .FirstOrDefaultAsync();

            return stats ?? new { UserId = userId, TotalCalories = 0, TotalMeals = 0 };
        }
    }
}
