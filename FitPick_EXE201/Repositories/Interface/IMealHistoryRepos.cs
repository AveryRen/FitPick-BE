using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IMealHistoryRepo
    {
        Task<IEnumerable<MealHistory>> GetUserHistoryAsync(int userId);
        Task<MealHistory?> GetByIdAsync(int id);
        Task AddAsync(MealHistory history);
        Task DeleteAsync(MealHistory history);
        Task<object> GetDailyStatsAsync(int userId, DateOnly date);
    }
}
