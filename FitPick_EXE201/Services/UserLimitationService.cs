using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class UserLimitationService
    {
        private readonly IUserPremiumRepo _userPremiumRepo;
        private readonly FitPickContext _context;

        public UserLimitationService(IUserPremiumRepo userPremiumRepo, FitPickContext context)
        {
            _userPremiumRepo = userPremiumRepo;
            _context = context;
        }

        /// <summary>
        /// Kiểm tra user có phải Premium hay không
        /// </summary>
        public async Task<bool> IsUserPremiumAsync(int userId)
        {
            return await _userPremiumRepo.IsUserPremiumAsync(userId);
        }

        /// <summary>
        /// Kiểm tra giới hạn xem món ăn cho Free user
        /// </summary>
        public async Task<bool> CanViewMealAsync(int userId, int mealId)
        {
            var isPremium = await IsUserPremiumAsync(userId);
            if (isPremium)
            {
                return true; // Premium user có thể xem tất cả
            }

            // Free user chỉ có thể xem một số món ăn nhất định
            var meal = await _context.Meals.FindAsync(mealId);
            if (meal == null) return false;

            // Free user chỉ có thể xem món ăn không phải premium
            return meal.IsPremium != true;
        }

        /// <summary>
        /// Lấy danh sách món ăn mà Free user có thể xem
        /// </summary>
        public async Task<List<Meal>> GetAvailableMealsForFreeUserAsync(int limit = 10)
        {
            return await _context.Meals
                .Where(m => m.IsPremium != true && m.StatusId == 1) // Chỉ món ăn free và active
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra giới hạn tạo meal plan cho Free user
        /// </summary>
        public async Task<bool> CanCreateMealPlanAsync(int userId)
        {
            var isPremium = await IsUserPremiumAsync(userId);
            if (isPremium)
            {
                return true; // Premium user có thể tạo không giới hạn
            }

            // Free user chỉ có thể tạo tối đa 3 meal plan trong ngày
            var today = DateTime.Now.Date;
            var todayPlansCount = await _context.Mealplans
                .Where(mp => mp.Userid == userId && mp.Date == DateOnly.FromDateTime(today))
                .CountAsync();

            return todayPlansCount < 3;
        }

        /// <summary>
        /// Kiểm tra giới hạn xem thực đơn tuần cho Free user
        /// </summary>
        public async Task<bool> CanViewWeeklyMealPlanAsync(int userId)
        {
            return await IsUserPremiumAsync(userId);
        }

        /// <summary>
        /// Lấy số lượng meal plan còn lại trong ngày cho Free user
        /// </summary>
        public async Task<int> GetRemainingMealPlansTodayAsync(int userId)
        {
            var isPremium = await IsUserPremiumAsync(userId);
            if (isPremium)
            {
                return int.MaxValue; // Premium user không giới hạn
            }

            var today = DateTime.Now.Date;
            var todayPlansCount = await _context.Mealplans
                .Where(mp => mp.Userid == userId && mp.Date == DateOnly.FromDateTime(today))
                .CountAsync();

            return Math.Max(0, 3 - todayPlansCount);
        }

        /// <summary>
        /// Kiểm tra giới hạn xem chi tiết món ăn premium
        /// </summary>
        public async Task<bool> CanViewMealDetailAsync(int userId, int mealId)
        {
            var isPremium = await IsUserPremiumAsync(userId);
            if (isPremium)
            {
                return true; // Premium user có thể xem tất cả
            }

            var meal = await _context.Meals.FindAsync(mealId);
            if (meal == null) return false;

            // Free user chỉ có thể xem chi tiết món ăn không phải premium
            return meal.IsPremium != true;
        }

        /// <summary>
        /// Lấy thông tin giới hạn của user
        /// </summary>
        public async Task<UserLimitationInfo> GetUserLimitationInfoAsync(int userId)
        {
            var isPremium = await IsUserPremiumAsync(userId);
            var remainingMealPlans = await GetRemainingMealPlansTodayAsync(userId);

            return new UserLimitationInfo
            {
                IsPremium = isPremium,
                CanViewAllMeals = isPremium,
                CanCreateUnlimitedMealPlans = isPremium,
                CanViewWeeklyMealPlan = isPremium,
                RemainingMealPlansToday = remainingMealPlans,
                MaxMealPlansPerDay = isPremium ? int.MaxValue : 3,
                MaxMealsToView = isPremium ? int.MaxValue : 10
            };
        }
    }

    public class UserLimitationInfo
    {
        public bool IsPremium { get; set; }
        public bool CanViewAllMeals { get; set; }
        public bool CanCreateUnlimitedMealPlans { get; set; }
        public bool CanViewWeeklyMealPlan { get; set; }
        public int RemainingMealPlansToday { get; set; }
        public int MaxMealPlansPerDay { get; set; }
        public int MaxMealsToView { get; set; }
    }
}
