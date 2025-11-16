using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using FitPick_EXE201.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class GenerateMealPlanResult
    {
        public bool Success { get; set; }
        public Mealplan? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }

    public class MealPlanService
    {
        private readonly IMealPlanRepo _mealPlanRepo;
        private readonly FitPickContext _context;
        private readonly UserService _userService;

        public MealPlanService(IMealPlanRepo mealPlanRepo, FitPickContext context, UserService userService)
        {
            _mealPlanRepo = mealPlanRepo;
            _context = context;
            _userService = userService;
        }

        public async Task<List<TodayMealPlanDto>> GetTodayMealPlanAsync(int userId, DateTime date)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid userId");
            var mealPlans = await _mealPlanRepo.GetTodayMealPlanAsync(userId, date);
            var orderedPlans = mealPlans.OrderBy(mp => mp.MealTime).ToList();
            return orderedPlans;
        }
        public Task<List<Mealplan>> GetUserMealPlansAsync(int userId) => _mealPlanRepo.GetUserMealPlansAsync(userId);

        public async Task<Mealplan?> GenerateMealPlanAsync(int userId, DateOnly date)
        {
            var plans = await _mealPlanRepo.GenerateMealPlanAsync(userId, date);
            if (plans == null || !plans.Any())
                return null;
            return plans.FirstOrDefault();
        }
        
        public Task<Mealplan> SwapMealAsync(int planId, int newMealId) => _mealPlanRepo.SwapMealAsync(planId, newMealId);

        public Task<bool> DeleteMealPlanAsync(int planId) => _mealPlanRepo.DeleteMealPlanAsync(planId);

        public Task<Mealplan?> ReplaceMealBySuggestionAsync(int planId, int userId) => 
            _mealPlanRepo.ReplaceMealBySuggestionAsync(planId, userId);

        public Task<Mealplan?> ReplaceMealByFavoritesAsync(int planId, int userId) => 
            _mealPlanRepo.ReplaceMealByFavoritesAsync(planId, userId);

        public Task<Mealplan?> AddMealToMenuAsync(int userId, int mealId, DateTime date, string? mealTime) =>
            _mealPlanRepo.AddMealToMenuAsync(userId, mealId, date, mealTime);

        // Generate meal plan with validation - sử dụng thông tin từ User (onboarding) và HealthProfile
        public async Task<GenerateMealPlanResult> GenerateMealPlanWithValidationAsync(int userId, DateOnly date)
        {
            try
            {
                // 1. Lấy thông tin User từ onboarding
                var user = await _context.Users
                    .Include(u => u.DietPlan)
                    .Include(u => u.CookingLevel)
                    .FirstOrDefaultAsync(u => u.Userid == userId);

                if (user == null)
                {
                    Console.WriteLine($"❌ User {userId} not found");
                    return new GenerateMealPlanResult
                    {
                        Success = false,
                        ErrorMessage = "Không tìm thấy thông tin người dùng.",
                        ErrorCode = "USER_NOT_FOUND"
                    };
                }

                // 2. Kiểm tra thông tin cơ bản từ onboarding
                if (!user.Age.HasValue || !user.Height.HasValue || !user.Weight.HasValue)
                {
                    Console.WriteLine($"❌ Incomplete onboarding: Age={user.Age}, Height={user.Height}, Weight={user.Weight}");
                    return new GenerateMealPlanResult
                    {
                        Success = false,
                        ErrorMessage = "Bạn chưa hoàn thành thông tin cá nhân trong onboarding. Vui lòng cập nhật thông tin (tuổi, chiều cao, cân nặng) trước khi tạo thực đơn.",
                        ErrorCode = "INCOMPLETE_ONBOARDING"
                    };
                }

                // 3. Tính target calories từ thông tin User (onboarding)
                int? targetCalories = null;

                // Ưu tiên lấy từ HealthProfile nếu có
                var healthProfile = await _context.Healthprofiles
                    .Include(hp => hp.Lifestyle)
                    .Include(hp => hp.Healthgoal)
                    .FirstOrDefaultAsync(hp => hp.Userid == userId);

                if (healthProfile != null && healthProfile.Targetcalories.HasValue && healthProfile.Targetcalories.Value > 0)
                {
                    targetCalories = healthProfile.Targetcalories.Value;
                }
                else
                {
                    // Tính từ thông tin User (onboarding)
                    targetCalories = await CalculateTargetCaloriesFromUserAsync(user, healthProfile);
                }

                if (!targetCalories.HasValue || targetCalories.Value <= 0)
                {
                    Console.WriteLine($"❌ Invalid target calories: {targetCalories}");
                    return new GenerateMealPlanResult
                    {
                        Success = false,
                        ErrorMessage = "Không thể tính toán target calories. Vui lòng kiểm tra lại thông tin cá nhân (tuổi, chiều cao, cân nặng).",
                        ErrorCode = "INVALID_TARGET_CALORIES"
                    };
                }

                // 4. Kiểm tra có meals phù hợp không
                var availableMeals = await _context.Meals
                    .Where(m => m.StatusId == 1 && (m.Calories ?? 0) > 0 && (m.Calories ?? 0) <= targetCalories.Value)
                    .CountAsync();

                if (availableMeals == 0)
                {
                    return new GenerateMealPlanResult
                    {
                        Success = false,
                        ErrorMessage = $"Không tìm thấy món ăn phù hợp với target calories của bạn ({targetCalories} cal). Vui lòng kiểm tra lại thông tin sức khỏe.",
                        ErrorCode = "NO_SUITABLE_MEALS"
                    };
                }

                // 5. Generate meal plan với target calories đã tính
                var plans = await _mealPlanRepo.GenerateMealPlanWithTargetCaloriesAsync(userId, date, targetCalories.Value);
                
                if (plans == null || !plans.Any())
                {
                    Console.WriteLine($"❌ No plans generated");
                    return new GenerateMealPlanResult
                    {
                        Success = false,
                        ErrorMessage = "Không thể tạo meal plan. Vui lòng thử lại sau.",
                        ErrorCode = "GENERATION_FAILED"
                    };
                }

                return new GenerateMealPlanResult
                {
                    Success = true,
                    Data = plans.FirstOrDefault()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GenerateMealPlanWithValidationAsync: {ex.Message}");
                return new GenerateMealPlanResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi server: {ex.Message}",
                    ErrorCode = "SERVER_ERROR"
                };
            }
        }

        // Tính target calories từ thông tin User (onboarding)
        private async Task<int?> CalculateTargetCaloriesFromUserAsync(User user, Healthprofile? healthProfile)
        {
            if (!user.Age.HasValue || !user.Height.HasValue || !user.Weight.HasValue)
            {
                Console.WriteLine($"❌ Missing user info for calculation");
                return null;
            }

            double weight = (double)user.Weight.Value;
            double height = (double)user.Height.Value;
            int age = user.Age.Value;
            int genderId = user.GenderId ?? 1; // Default to Male

            // Tính BMR (Basal Metabolic Rate) - sử dụng công thức Harris-Benedict
            double bmr = 0;
            if (genderId == 1) // Male
            {
                bmr = 88.362 + (13.397 * weight) + (4.799 * height) - (5.677 * age);
            }
            else // Female
            {
                bmr = 447.593 + (9.247 * weight) + (3.098 * height) - (4.330 * age);
            }

            // Hệ số hoạt động - lấy từ health profile hoặc default
            double activityMultiplier = 1.2; // Sedentary default
            if (healthProfile?.Lifestyleid.HasValue == true)
            {
                var lifestyle = await _context.Lifestyles.FirstOrDefaultAsync(l => l.Id == healthProfile.Lifestyleid.Value);
                if (lifestyle != null)
                {
                    activityMultiplier = (double)lifestyle.Multiplier;
                }
            }

            double targetCalories = bmr * activityMultiplier;

            // Điều chỉnh theo health goal nếu có
            if (healthProfile?.Healthgoalid.HasValue == true)
            {
                var goal = await _context.Healthgoals.FirstOrDefaultAsync(g => g.Id == healthProfile.Healthgoalid.Value);
                if (goal != null)
                {
                    targetCalories += goal.CalorieAdjustment;
                }
            }

            int result = (int)Math.Round(targetCalories);
            return result;
        }
    }
}
