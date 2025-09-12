using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

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

        public async Task<MealHistory?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddMealHistoryAsync(MealHistory history)
        {
            history.Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            await _repository.AddAsync(history);
        }

        public async Task DeleteMealHistoryAsync(int id)
        {
            var history = await _repository.GetByIdAsync(id);
            if (history != null)
            {
                await _repository.DeleteAsync(history);
            }
        }

        public async Task<object> GetDailyStatsAsync(int userId, DateOnly date)
        {
            return await _repository.GetDailyStatsAsync(userId, date);
        }
    }
}
