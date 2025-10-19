using FitPick_EXE201.Models.DTOs;

namespace FitPick_EXE201.Services
{
    public interface IMealDetailService
    {
        Task<MealDetailDto?> GetMealDetailByIdAsync(int mealId);
    }
}
