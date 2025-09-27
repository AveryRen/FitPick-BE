using FitPick_EXE201.Models.DTOs;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IMealPlanRepo
    {
        Task<List<TodayMealPlanDto>> GetTodayMealPlanAsync(int userId, DateTime date);
    }
}
