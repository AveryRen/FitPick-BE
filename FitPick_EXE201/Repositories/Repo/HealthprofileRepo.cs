using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class HealthprofileRepo : BaseRepo<Healthprofile, int>, IHealthprofileRepo
    {
        private readonly FitPickContext _context;
        public HealthprofileRepo(FitPickContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Healthprofile?> GetByUserIdAsync(int id)
        {
            return await _context.Healthprofiles
                .Include(h => h.Lifestyle)
                .Include(h => h.Healthgoal)
                .FirstOrDefaultAsync(h => h.Userid == id);
        }
        public async Task<ProgressDto?> GetUserProgressAsync(int userId)
        {
            var profile = await _context.Healthprofiles
                .Include(h => h.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Userid == userId && h.Status == true);

            if (profile == null) return null;

            var today = DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Unspecified);
            double currentCalories = 0;

            if (_context.MealHistories != null)
            {
                currentCalories = await _context.MealHistories
                    .Where(m => m.Userid == userId
                             && m.ConsumedAt.HasValue
                             && m.ConsumedAt.Value.Date == today)
                    .SumAsync(m => (double?)(m.Calories ?? 0)) ?? 0;
            }

            return new ProgressDto
            {
                CurrentWeight = profile.User?.Weight,
                TargetWeight = profile.Targetweight,
                CurrentCalories = currentCalories,
                TargetCalories = profile.Targetcalories
            };
        }
        public async Task<UserGoalDto?> GetUserGoalAsync(int userId)
        {
            var profile = await _context.Healthprofiles
                .Include(h => h.Healthgoal)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Userid == userId && h.Status == true);

            if (profile == null) return null;

            return new UserGoalDto
            {
                UserId = profile.Userid ?? userId,
                TargetWeight = profile.Targetweight,
                TargetCalories = profile.Targetcalories,
                GoalName = profile.Healthgoal?.Name
            };
        }

        public async Task<NutritionStatsDto?> GetNutritionStatsAsync(int userId, DateTime? date = null)
        {
            var profile = await _context.Healthprofiles
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Userid == userId && h.Status == true);

            if (profile == null) return null;

            var targetDate = date ?? DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Unspecified);
            
            // Get today's meal history with meal details
            var todayMeals = await _context.MealHistories
                .Include(mh => mh.Meal)
                .Where(m => m.Userid == userId
                         && m.ConsumedAt.HasValue
                         && m.ConsumedAt.Value.Date == targetDate)
                .ToListAsync();

            // Calculate consumed calories and macros from meals
            double consumedCalories = todayMeals.Sum(m => (double)(m.Calories ?? 0));
            double consumedCarbs = todayMeals.Sum(m => (double)(m.Meal?.Carbs ?? 0));
            double consumedProtein = todayMeals.Sum(m => (double)(m.Meal?.Protein ?? 0));
            double consumedFat = todayMeals.Sum(m => (double)(m.Meal?.Fat ?? 0));

            // Calculate macro targets based on target calories
            // Standard macro ratio: 50% carbs, 30% protein, 20% fat
            // 1g carbs = 4 kcal, 1g protein = 4 kcal, 1g fat = 9 kcal
            int targetCalories = profile.Targetcalories ?? 2000;
            double targetCarbs = (targetCalories * 0.50) / 4;  // 50% of calories from carbs
            double targetProtein = (targetCalories * 0.30) / 4; // 30% of calories from protein
            double targetFat = (targetCalories * 0.20) / 9;     // 20% of calories from fat

            return new NutritionStatsDto
            {
                TargetCalories = targetCalories,
                ConsumedCalories = consumedCalories,
                Starch = new MacroNutrientDto
                {
                    Current = Math.Round(consumedCarbs, 1),
                    Target = Math.Round(targetCarbs, 1)
                },
                Protein = new MacroNutrientDto
                {
                    Current = Math.Round(consumedProtein, 1),
                    Target = Math.Round(targetProtein, 1)
                },
                Fat = new MacroNutrientDto
                {
                    Current = Math.Round(consumedFat, 1),
                    Target = Math.Round(targetFat, 1)
                }
            };
        }
    }
}
