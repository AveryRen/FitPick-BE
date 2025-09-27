using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class MealPlanService
    {
        private readonly IMealPlanRepo _mealPlanRepo;

        public MealPlanService(IMealPlanRepo mealPlanRepo)
        {
            _mealPlanRepo = mealPlanRepo;
        }

        public async Task<List<TodayMealPlanDto>> GetTodayMealPlanAsync(int userId, DateTime date)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid userId");
            var mealPlans = await _mealPlanRepo.GetTodayMealPlanAsync(userId, date);
            var orderedPlans = mealPlans.OrderBy(mp => mp.MealTime).ToList();
            return orderedPlans;
        }
    }
}
