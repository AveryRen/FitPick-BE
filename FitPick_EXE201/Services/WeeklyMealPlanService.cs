using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class WeeklyMealPlanService
    {
        private readonly IUserPremiumRepo _userPremiumRepo;
        private readonly FitPickContext _context;

        public WeeklyMealPlanService(IUserPremiumRepo userPremiumRepo, FitPickContext context)
        {
            _userPremiumRepo = userPremiumRepo;
            _context = context;
        }

        /// <summary>
        /// Tạo thực đơn tuần cho Premium user
        /// </summary>
    public async Task<WeeklyMealPlanDto?> GenerateWeeklyMealPlanAsync(int userId, DateTime weekStartDate)
        {
            // Kiểm tra user có phải Premium không
            var isPremium = await _userPremiumRepo.IsUserPremiumAsync(userId);
            if (!isPremium)
            {
                throw new UnauthorizedAccessException("Chỉ Premium user mới có thể tạo thực đơn tuần");
            }

            // Xóa thực đơn tuần cũ nếu có (chỉ từ hôm nay trở đi, không xóa lịch sử)
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Normalize to Monday-start week regardless of input
            var normalizedStart = weekStartDate.Date;
            int diffToMonday = ((int)normalizedStart.DayOfWeek + 6) % 7; // 0 for Monday
            normalizedStart = normalizedStart.AddDays(-diffToMonday);
            var weekEndDate = normalizedStart.AddDays(6);
            
            var existingPlans = await _context.Mealplans
                .Where(mp => mp.Userid == userId && 
                           mp.Date >= today && // Chỉ xóa từ hôm nay trở đi
                           mp.Date >= DateOnly.FromDateTime(normalizedStart) && 
                           mp.Date <= DateOnly.FromDateTime(weekEndDate))
                .ToListAsync();

            if (existingPlans.Any())
            {
                Console.WriteLine($"🗑️ Removing {existingPlans.Count} existing meal plans from today onwards");
                _context.Mealplans.RemoveRange(existingPlans);
            }

            // Lấy thông tin user profile
            var profile = await _context.Healthprofiles
                .Include(hp => hp.Healthgoal)
                .Include(hp => hp.Lifestyle)
                .FirstOrDefaultAsync(hp => hp.Userid == userId);

            if (profile == null)
            {
                throw new InvalidOperationException("Không tìm thấy thông tin sức khỏe của user");
            }

            // Lấy danh sách món ăn phù hợp
            var availableMeals = await _context.Meals
                .Where(m => m.StatusId == 1) // Chỉ món ăn active
                .ToListAsync();

            if (!availableMeals.Any())
            {
                throw new InvalidOperationException("Không có món ăn nào khả dụng");
            }

            // Lấy thời gian bữa ăn
            var mealTimes = await _context.MealTimes
                .OrderBy(mt => mt.Id)
                .Take(3)
                .ToListAsync();
            var random = new Random();

            var weeklyPlans = new List<Mealplan>();
            var dailyPlans = new List<WeeklyDailyMealPlanDto>();

            // Xác định ngày bắt đầu tạo: từ hôm nay hoặc từ thứ Hai, tùy ngày nào muộn hơn
            var startForGeneration = DateOnly.FromDateTime(normalizedStart) < today
                ? today
                : DateOnly.FromDateTime(normalizedStart);
            var endForGeneration = DateOnly.FromDateTime(weekEndDate);

            for (var d = startForGeneration; d <= endForGeneration; d = d.AddDays(1))
            {
                var currentDate = d.ToDateTime(TimeOnly.MinValue);

                var dayPlans = new List<TodayMealPlanDto>();

                // Tạo 3 bữa ăn cho mỗi ngày
                foreach (var mealTime in mealTimes)
                {
                    // Chọn 2 món ăn ngẫu nhiên cho mỗi bữa
                    var selectedMeals = availableMeals
                        .OrderBy(x => random.Next())
                        .Take(2)
                        .ToList();

                    foreach (var meal in selectedMeals)
                    {
                        var mealPlan = new Mealplan
                        {
                            Userid = userId,
                            Date = DateOnly.FromDateTime(currentDate),
                            MealtimeId = mealTime.Id,
                            Mealid = meal.Mealid,
                            StatusId = 1 // Active
                        };

                        weeklyPlans.Add(mealPlan);

                        // Tạo DTO cho response
                        var todayMealPlan = new TodayMealPlanDto
                        {
                            PlanId = 0, // Sẽ được cập nhật sau khi save
                            Date = currentDate,
                            MealTime = mealTime.Name ?? "Unknown",
                            Meal = new MealDto
                            {
                                Mealid = meal.Mealid,
                                Name = meal.Name,
                                Description = meal.Description,
                                Calories = meal.Calories,
                                Protein = meal.Protein ?? 0m,
                                Carbs = meal.Carbs ?? 0m,
                                Fat = meal.Fat ?? 0m,
                                Cookingtime = meal.Cookingtime,
                                Diettype = meal.Diettype,
                                Price = meal.Price,
                                ImageUrl = meal.ImageUrl,
                                IsPremium = meal.IsPremium
                            }
                        };

                        dayPlans.Add(todayMealPlan);
                    }
                }

                dailyPlans.Add(new WeeklyDailyMealPlanDto
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    DayName = currentDate.ToString("dddd"),
                    Meals = dayPlans
                });
            }

            // Lưu vào database
            _context.Mealplans.AddRange(weeklyPlans);
            await _context.SaveChangesAsync();

            // Cập nhật PlanId cho các DTO
            for (int i = 0; i < weeklyPlans.Count; i++)
            {
                var dayIndex = i / 6; // 6 meals per day (3 meal times * 2 meals each)
                var mealIndex = i % 6;
                if (dayIndex < dailyPlans.Count && mealIndex < dailyPlans[dayIndex].Meals.Count)
                {
                    dailyPlans[dayIndex].Meals[mealIndex].PlanId = weeklyPlans[i].Planid;
                }
            }

            return new WeeklyMealPlanDto
            {
                WeekStartDate = normalizedStart.ToString("yyyy-MM-dd"),
                WeekEndDate = weekEndDate.ToString("yyyy-MM-dd"),
                UserId = userId,
                DailyPlans = dailyPlans,
                TotalCalories = CalculateTotalCalories(dailyPlans),
                GeneratedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Lấy thực đơn tuần hiện tại
        /// </summary>
        public async Task<WeeklyMealPlanDto?> GetCurrentWeeklyMealPlanAsync(int userId)
        {
            var isPremium = await _userPremiumRepo.IsUserPremiumAsync(userId);
            if (!isPremium)
            {
                throw new UnauthorizedAccessException("Chỉ Premium user mới có thể xem thực đơn tuần");
            }

            var today = DateTime.Today;
            // Monday-start week for current view
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7; // 0 for Monday
            var weekStart = today.AddDays(-diffToMonday);
            var weekEnd = weekStart.AddDays(6);

            var weeklyPlans = await _context.Mealplans
                .Include(mp => mp.Meal)
                .Include(mp => mp.Mealtime)
                .Where(mp => mp.Userid == userId && 
                           mp.Date >= DateOnly.FromDateTime(weekStart) && 
                           mp.Date <= DateOnly.FromDateTime(weekEnd))
                .OrderBy(mp => mp.Date)
                .ThenBy(mp => mp.MealtimeId)
                .ToListAsync();

            if (!weeklyPlans.Any())
            {
                return null;
            }

            var dailyPlans = new List<WeeklyDailyMealPlanDto>();
            var currentDate = weekStart;

            for (int day = 0; day < 7; day++)
            {
                var dayPlans = weeklyPlans
                    .Where(mp => mp.Date == DateOnly.FromDateTime(currentDate))
                    .Select(mp => new TodayMealPlanDto
                    {
                        PlanId = mp.Planid,
                        Date = currentDate,
                        MealTime = mp.Mealtime?.Name ?? "Unknown",
                        Meal = new MealDto
                        {
                            Mealid = mp.Meal!.Mealid,
                            Name = mp.Meal!.Name,
                            Description = mp.Meal!.Description,
                            Calories = mp.Meal!.Calories,
                            Protein = mp.Meal!.Protein ?? 0m,
                            Carbs = mp.Meal!.Carbs ?? 0m,
                            Fat = mp.Meal!.Fat ?? 0m,
                            Cookingtime = mp.Meal!.Cookingtime,
                            Diettype = mp.Meal!.Diettype,
                            Price = mp.Meal!.Price,
                            ImageUrl = mp.Meal!.ImageUrl,
                            IsPremium = mp.Meal!.IsPremium
                        }
                    })
                    .ToList();

                dailyPlans.Add(new WeeklyDailyMealPlanDto
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    DayName = currentDate.ToString("dddd"),
                    Meals = dayPlans
                });

                currentDate = currentDate.AddDays(1);
            }

            return new WeeklyMealPlanDto
            {
                WeekStartDate = weekStart.ToString("yyyy-MM-dd"),
                WeekEndDate = weekEnd.ToString("yyyy-MM-dd"),
                UserId = userId,
                DailyPlans = dailyPlans,
                TotalCalories = CalculateTotalCalories(dailyPlans),
                GeneratedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Tính tổng calories cho cả tuần
        /// </summary>
        private int CalculateTotalCalories(List<WeeklyDailyMealPlanDto> dailyPlans)
        {
            return dailyPlans.Sum(day => 
                day.Meals.Sum(meal => meal.Meal.Calories ?? 0)
            );
        }
    }

    public class WeeklyMealPlanDto
    {
        public string WeekStartDate { get; set; } = string.Empty;
        public string WeekEndDate { get; set; } = string.Empty;
        public int UserId { get; set; }
        public List<WeeklyDailyMealPlanDto> DailyPlans { get; set; } = new List<WeeklyDailyMealPlanDto>();
        public int TotalCalories { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class WeeklyDailyMealPlanDto
    {
        public string Date { get; set; } = string.Empty;
        public string DayName { get; set; } = string.Empty;
        public List<TodayMealPlanDto> Meals { get; set; } = new List<TodayMealPlanDto>();
    }
}
