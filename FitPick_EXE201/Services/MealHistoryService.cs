using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class MealHistoryService
    {
        private readonly IMealHistoryRepo _repository;

        public MealHistoryService(IMealHistoryRepo repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MealHistory>> GetUserHistoryAsync(int userId)
        {
            return await _repository.GetUserHistoryAsync(userId);
        }

        public async Task<IEnumerable<MealHistory>> GetUserHistoryByDateAsync(int userId, DateOnly date)
        {
            return await _repository.GetUserHistoryByDateAsync(userId, date);
        }

        public async Task<MealHistory?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddMealHistoryAsync(MealHistory history)
        {
            history.Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            await _repository.AddAsync(history);
        }

        public async Task<bool> DeleteMealHistoryAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<object> GetDailyStatsAsync(int userId, DateOnly date)
        {
            return await _repository.GetDailyStatsAsync(userId, date);
        }

        public async Task<object> GetDetailedDailyStatsAsync(int userId, DateOnly date)
        {
            return await _repository.GetDetailedDailyStatsAsync(userId, date);
        }

        public async Task<bool> IsMealEatenTodayAsync(int userId, int mealId, DateOnly date)
        {
            return await _repository.IsMealEatenTodayAsync(userId, mealId, date);
        }

        public async Task<MealHistory?> GetMealHistoryByMealAndDateAsync(int userId, int mealId, DateOnly date)
        {
            return await _repository.GetMealHistoryByMealAndDateAsync(userId, mealId, date);
        }
    }
}
