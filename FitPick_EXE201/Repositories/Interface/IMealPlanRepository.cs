using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IMealPlanRepo
    {
        Task<List<TodayMealPlanDto>> GetTodayMealPlanAsync(int userId, DateTime date);
        Task<List<Mealplan>> GetUserMealPlansAsync(int userId);

        Task<List<Mealplan>> GenerateMealPlanAsync(int userId, DateOnly date);

        Task<Mealplan?> SwapMealAsync(int planId, int newMealId);

        Task<bool> DeleteMealPlanAsync(int planId);
    }
}
