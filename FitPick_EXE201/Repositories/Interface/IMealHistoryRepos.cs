using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IMealHistoryRepo
    {
        Task<IEnumerable<MealHistory>> GetUserHistoryAsync(int userId);
        Task<IEnumerable<MealHistory>> GetUserHistoryByDateAsync(int userId, DateOnly date);
        Task<MealHistory?> GetByIdAsync(int id);
        Task AddAsync(MealHistory history);
        Task<bool> DeleteAsync(int id);
        Task<object> GetDailyStatsAsync(int userId, DateOnly date);
        Task<object> GetDetailedDailyStatsAsync(int userId, DateOnly date);
        Task<bool> IsMealEatenTodayAsync(int userId, int mealId, DateOnly date);
        Task<MealHistory?> GetMealHistoryByMealAndDateAsync(int userId, int mealId, DateOnly date);
    }
}
