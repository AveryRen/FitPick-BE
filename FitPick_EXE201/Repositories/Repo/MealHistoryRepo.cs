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

        public async Task<IEnumerable<MealHistory>> GetUserHistoryByDateAsync(int userId, DateOnly date)
        {
            return await _context.MealHistories
                .Where(h => h.Userid == userId && h.Date == date)
                .Include(h => h.Meal)
                .Include(h => h.Mealtime)
                .OrderBy(h => h.MealtimeId)
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

        public async Task<bool> DeleteAsync(int id)
        {
            var history = await _context.MealHistories.FindAsync(id);
            if (history == null) return false;

            _context.MealHistories.Remove(history);
            await _context.SaveChangesAsync();
            return true;
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

        public async Task<object> GetDetailedDailyStatsAsync(int userId, DateOnly date)
        {
            var histories = await _context.MealHistories
                .Where(h => h.Userid == userId && h.Date == date)
                .Include(h => h.Meal)
                .Include(h => h.Mealtime)
                .ToListAsync();

            var totalCalories = histories.Sum(h => h.Calories ?? 0);
            var totalProtein = histories.Sum(h => (h.Meal?.Protein ?? 0) * (h.Quantity ?? 1));
            var totalCarbs = histories.Sum(h => (h.Meal?.Carbs ?? 0) * (h.Quantity ?? 1));
            var totalFat = histories.Sum(h => (h.Meal?.Fat ?? 0) * (h.Quantity ?? 1));

            var mealsByTime = histories.GroupBy(h => h.Mealtime?.Name ?? "Unknown")
                .Select(g => new
                {
                    MealTime = g.Key,
                    Calories = g.Sum(h => h.Calories ?? 0),
                    Count = g.Count()
                })
                .ToList();

            return new
            {
                Date = date,
                TotalCalories = totalCalories,
                TotalProtein = totalProtein,
                TotalCarbs = totalCarbs,
                TotalFat = totalFat,
                TotalMeals = histories.Count,
                MealsByTime = mealsByTime,
                MealHistories = histories.Select(h => new
                {
                    h.Historyid,
                    h.Mealid,
                    MealName = h.Meal?.Name,
                    MealTime = h.Mealtime?.Name,
                    h.Quantity,
                    h.Calories,
                    h.Createdat
                })
            };
        }

        public async Task<bool> IsMealEatenTodayAsync(int userId, int mealId, DateOnly date)
        {
            return await _context.MealHistories
                .AnyAsync(h => h.Userid == userId && h.Mealid == mealId && h.Date == date);
        }

        public async Task<MealHistory?> GetMealHistoryByMealAndDateAsync(int userId, int mealId, DateOnly date)
        {
            return await _context.MealHistories
                .Include(h => h.Meal)
                .Include(h => h.Mealtime)
                .FirstOrDefaultAsync(h => h.Userid == userId && h.Mealid == mealId && h.Date == date);
        }
    }
}
