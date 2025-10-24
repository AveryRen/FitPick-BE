using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class ProPersonalizedService
    {
        private readonly FitPickContext _context;
        private readonly IUserPremiumRepo _premiumRepo;
        private readonly IReminderRepo _reminderRepo;
        private readonly AiService _aiService;

        public ProPersonalizedService(
            FitPickContext context,
            IUserPremiumRepo premiumRepo,
            IReminderRepo reminderRepo,
            AiService aiService)
        {
            _context = context;
            _premiumRepo = premiumRepo;
            _reminderRepo = reminderRepo;
            _aiService = aiService;
        }

        /// <summary>
        /// Kiểm tra user có phải Premium không
        /// </summary>
        private async Task<bool> IsPremiumUserAsync(int userId)
        {
            return await _premiumRepo.IsUserPremiumAsync(userId);
        }

        /// <summary>
        /// Tạo gợi ý cá nhân hóa chuyên sâu dựa trên AI và lịch sử user
        /// </summary>
        public async Task<DeepRecommendationDto?> GetDeepPersonalizedRecommendationsAsync(int userId)
        {
            if (!await IsPremiumUserAsync(userId))
                return null;

            // Lấy thông tin user và lịch sử
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Userid == userId);

            var healthProfile = await _context.Healthprofiles
                .Include(hp => hp.Healthgoal)
                .Include(hp => hp.Lifestyle)
                .FirstOrDefaultAsync(hp => hp.Userid == userId);

            if (healthProfile == null)
                return new DeepRecommendationDto
                {
                    PersonalizedMessage = "Vui lòng cập nhật hồ sơ sức khỏe để nhận gợi ý cá nhân hóa."
                };

            // Lấy lịch sử meal plans (30 ngày gần nhất)
            var mealHistory = await _context.Mealplans
                .Include(mp => mp.Meal)
                .Where(mp => mp.Userid == userId && 
                           mp.Date >= DateOnly.FromDateTime(DateTime.Now.AddDays(-30)))
                .OrderByDescending(mp => mp.Date)
                .Take(100)
                .ToListAsync();

            // Lấy favorite meals
            var favorites = await _context.MealFavorites
                .Include(mf => mf.Meal)
                .Where(mf => mf.UserId == userId)
                .Select(mf => mf.Meal!.Name)
                .ToListAsync();

            // Phân tích xu hướng
            var avgCalories = mealHistory.Any() 
                ? mealHistory.Average(m => m.Meal?.Calories ?? 0) 
                : 0;

            var profile = healthProfile;
            var targetCalories = profile.Targetcalories ?? 2000;
            
            // Lấy cân nặng thực từ User entity
            var currentWeight = user?.Weight ?? 70m; // Cân nặng hiện tại từ User
            var targetWeight = profile.Targetweight ?? user?.TargetWeight ?? 0; // Target từ profile hoặc user
            var initialWeight = currentWeight; // Tạm dùng currentWeight làm initial, sau này có thể lưu riêng
            var goalName = profile.Healthgoal?.Name ?? "Unknown";

            // Tạo personalized message
            var personalizedMessage = $"Xin chào {user?.Fullname ?? "bạn"}! " +
                $"Dựa trên mục tiêu '{goalName}' của bạn và lịch sử ăn uống, " +
                $"chúng tôi đã chọn những món ăn phù hợp nhất.";

            // Lấy recommended meals dựa trên profile
            var recommendedMeals = await GetSmartRecommendedMealsAsync(
                userId, 
                targetCalories, 
                goalName, 
                favorites,
                mealHistory.Select(m => m.Mealid ?? 0).Where(id => id > 0).ToList());

            // Tạo nutrition tips
            var nutritionTips = GenerateNutritionTips(profile, (decimal)avgCalories);

            // Tạo goal-based advice
            var goalAdvice = GenerateGoalBasedAdvice(profile, mealHistory.Count);

            // Tính progress dựa trên cân nặng thực
            var weightDifference = Math.Abs(currentWeight - targetWeight);
            var totalWeightToChange = Math.Abs(initialWeight - targetWeight);
            var progressPercent = totalWeightToChange > 0 
                ? Math.Min(100, (1 - (weightDifference / totalWeightToChange)) * 100)
                : 0;

            // Tạo progress summary với dữ liệu thực từ User
            var progressSummary = new ProgressSummary
            {
                GoalName = goalName,
                CurrentWeight = currentWeight,
                TargetWeight = targetWeight,
                WeightChange = initialWeight - currentWeight, // Đã thay đổi bao nhiêu từ lúc bắt đầu
                DaysActive = mealHistory.Select(m => m.Date).Distinct().Count(),
                AverageCaloriesPerDay = (decimal)avgCalories,
                ProgressPercentage = $"{progressPercent:F1}%"
            };

            return new DeepRecommendationDto
            {
                PersonalizedMessage = personalizedMessage,
                RecommendedMeals = recommendedMeals,
                NutritionTips = nutritionTips,
                GoalBasedAdvice = goalAdvice,
                Progress = progressSummary
            };
        }

        /// <summary>
        /// Lấy món ăn được đề xuất thông minh
        /// </summary>
        private async Task<List<RecommendedMealDto>> GetSmartRecommendedMealsAsync(
            int userId,
            decimal targetCalories,
            string goal,
            List<string> favorites,
            List<int> recentMealIds)
        {
            var meals = await _context.Meals
                .Where(m => m.StatusId == 1)
                .ToListAsync();

            var recommended = new List<RecommendedMealDto>();

            foreach (var meal in meals.Take(20))
            {
                var matchScore = CalculateMatchScore(
                    meal, 
                    targetCalories, 
                    goal, 
                    favorites, 
                    recentMealIds);

                if (matchScore >= 50) // Chỉ lấy món có điểm >= 50
                {
                    recommended.Add(new RecommendedMealDto
                    {
                        MealId = meal.Mealid,
                        Name = meal.Name,
                        Calories = meal.Calories,
                        ImageUrl = meal.ImageUrl,
                        MatchScore = matchScore,
                        Reason = GenerateRecommendationReason(meal, goal, favorites, matchScore),
                        MealTimeRecommendation = DetermineMealTime(meal.Calories ?? 0)
                    });
                }
            }

            return recommended.OrderByDescending(r => r.MatchScore).Take(10).ToList();
        }

        /// <summary>
        /// Tính điểm phù hợp của món ăn
        /// </summary>
        private decimal CalculateMatchScore(
            Meal meal,
            decimal targetCalories,
            string goal,
            List<string> favorites,
            List<int> recentMealIds)
        {
            decimal score = 50; // Base score

            // Điểm calories phù hợp
            var caloriesPerMeal = targetCalories / 3; // Chia 3 bữa
            var caloriesDiff = Math.Abs((meal.Calories ?? 0) - caloriesPerMeal);
            var caloriesScore = Math.Max(0, 30 - (caloriesDiff / caloriesPerMeal * 30));
            score += caloriesScore;

            // Điểm yêu thích
            if (favorites.Contains(meal.Name))
                score += 15;

            // Trừ điểm nếu đã ăn gần đây
            if (recentMealIds.Contains(meal.Mealid))
                score -= 10;

            // Điểm goal phù hợp
            if (goal.Contains("giảm") && (meal.Calories ?? 0) < caloriesPerMeal)
                score += 10;
            else if (goal.Contains("tăng") && (meal.Calories ?? 0) > caloriesPerMeal)
                score += 10;

            // Điểm protein cao cho muscle gain
            if (goal.Contains("cơ") && (meal.Protein ?? 0) > 20)
                score += 5;

            return Math.Min(100, Math.Max(0, score));
        }

        /// <summary>
        /// Tạo lý do recommend
        /// </summary>
        private string GenerateRecommendationReason(Meal meal, string goal, List<string> favorites, decimal score)
        {
            var reasons = new List<string>();

            if (favorites.Contains(meal.Name))
                reasons.Add("Trong danh sách yêu thích của bạn");

            if (goal.Contains("giảm") && (meal.Calories ?? 0) < 400)
                reasons.Add("Ít calories phù hợp giảm cân");
            else if (goal.Contains("tăng") && (meal.Calories ?? 0) > 500)
                reasons.Add("Đủ calories cho tăng cân");

            if ((meal.Protein ?? 0) > 20)
                reasons.Add("Giàu protein");

            if (score >= 80)
                reasons.Add("Rất phù hợp với mục tiêu");
            else if (score >= 60)
                reasons.Add("Phù hợp với mục tiêu");

            return reasons.Any() ? string.Join(", ", reasons) : "Được đề xuất cho bạn";
        }

        /// <summary>
        /// Xác định bữa ăn phù hợp
        /// </summary>
        private string DetermineMealTime(decimal calories)
        {
            if (calories < 350) return "Bữa sáng";
            if (calories < 550) return "Bữa trưa";
            return "Bữa tối";
        }

        /// <summary>
        /// Tạo nutrition tips
        /// </summary>
        private List<string> GenerateNutritionTips(Healthprofile profile, decimal avgCalories)
        {
            var tips = new List<string>();
            var targetCalories = profile.Targetcalories ?? 2000;

            if (avgCalories > targetCalories + 200)
                tips.Add($"Lượng calories trung bình ({avgCalories:F0}) cao hơn mục tiêu ({targetCalories}). Cân nhắc giảm khẩu phần.");
            else if (avgCalories < targetCalories - 200)
                tips.Add($"Lượng calories trung bình ({avgCalories:F0}) thấp hơn mục tiêu ({targetCalories}). Cân nhắc tăng khẩu phần.");
            else
                tips.Add($"Lượng calories trung bình của bạn đang ở mức tốt!");

            tips.Add("Uống đủ 2-3 lít nước mỗi ngày");
            tips.Add("Ăn nhiều rau xanh và trái cây tươi");
            tips.Add("Hạn chế đồ chiên rán và đường tinh luyện");

            return tips;
        }

        /// <summary>
        /// Tạo goal-based advice
        /// </summary>
        private List<string> GenerateGoalBasedAdvice(Healthprofile profile, int activeDays)
        {
            var advice = new List<string>();
            var goalName = profile.Healthgoal?.Name ?? "";

            if (goalName.Contains("giảm"))
            {
                advice.Add("Duy trì thâm hụt calories nhẹ (300-500 cal/ngày)");
                advice.Add("Tăng hoạt động cardio 3-4 lần/tuần");
                advice.Add("Ưu tiên protein và chất xơ");
            }
            else if (goalName.Contains("tăng"))
            {
                advice.Add("Duy trì thặng dư calories (300-500 cal/ngày)");
                advice.Add("Tập trung vào bài tập sức mạnh");
                advice.Add("Ăn nhiều protein (1.6-2g/kg cân nặng)");
            }
            else
            {
                advice.Add("Duy trì chế độ ăn cân bằng");
                advice.Add("Kết hợp cả cardio và sức mạnh");
            }

            if (activeDays < 7)
                advice.Add("Hãy duy trì kế hoạch ăn uống đều đặn hơn!");

            return advice;
        }

        /// <summary>
        /// Tính % progress
        /// </summary>
        private string CalculateProgressPercentage(decimal current, decimal target, decimal initial)
        {
            if (initial == target) return "100%";
            var progress = ((initial - current) / (initial - target)) * 100;
            return $"{Math.Max(0, Math.Min(100, progress)):F1}%";
        }

        /// <summary>
        /// Thiết lập nhắc nhở tự động cho các bữa ăn
        /// </summary>
        public async Task<List<ReminderResponseDto>?> SetupAutomaticMealRemindersAsync(int userId)
        {
            if (!await IsPremiumUserAsync(userId))
                return null;

            // Xóa các reminder cũ về meal
            var oldReminders = await _context.Notifications
                .Where(n => n.Userid == userId && 
                           n.TypeId == 1 && 
                           n.Title != null &&
                           n.Title.Contains("bữa"))
                .ToListAsync();

            _context.Notifications.RemoveRange(oldReminders);

            // Tạo reminder mới cho 3 bữa ăn
            var mealTimes = new[]
            {
                new { Title = "🌅 Nhắc nhở bữa sáng", Message = "Đã đến giờ bữa sáng! Hãy chọn món ăn phù hợp với mục tiêu của bạn.", Hour = 7, Minute = 0 },
                new { Title = "☀️ Nhắc nhở bữa trưa", Message = "Đã đến giờ bữa trưa! Đừng quên ăn đủ chất để duy trì năng lượng.", Hour = 12, Minute = 0 },
                new { Title = "🌙 Nhắc nhở bữa tối", Message = "Đã đến giờ bữa tối! Hãy ăn nhẹ và lành mạnh.", Hour = 18, Minute = 30 }
            };

            var createdReminders = new List<ReminderResponseDto>();
            var today = DateTime.Today;

            foreach (var mealTime in mealTimes)
            {
                var scheduleTime = new DateTime(
                    today.Year, 
                    today.Month, 
                    today.Day, 
                    mealTime.Hour, 
                    mealTime.Minute, 
                    0);

                // Nếu giờ đã qua hôm nay, lên lịch cho ngày mai
                if (scheduleTime < DateTime.Now)
                    scheduleTime = scheduleTime.AddDays(1);

                var reminder = new Notification
                {
                    Userid = userId,
                    Title = mealTime.Title,
                    Message = mealTime.Message,
                    TypeId = 1, // Reminder type
                    Scheduledat = DateTime.SpecifyKind(scheduleTime, DateTimeKind.Unspecified),
                    IsDone = false,
                    Isread = false,
                    Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };

                _context.Notifications.Add(reminder);
            }

            await _context.SaveChangesAsync();

            // Lấy lại reminders vừa tạo để trả về
            var savedReminders = await _context.Notifications
                .Where(n => n.Userid == userId && 
                           n.TypeId == 1 && 
                           n.Createdat > DateTime.Now.AddMinutes(-1))
                .Select(n => new ReminderResponseDto
                {
                    Notificationid = n.Notificationid,
                    Title = n.Title,
                    Message = n.Message,
                    Scheduledat = n.Scheduledat,
                    IsDone = n.IsDone,
                    Isread = n.Isread
                })
                .ToListAsync();

            return savedReminders;
        }

        /// <summary>
        /// Phân tích dinh dưỡng chi tiết
        /// </summary>
        public async Task<NutritionInsightsDto?> GetNutritionInsightsAsync(int userId, int days)
        {
            if (!await IsPremiumUserAsync(userId))
                return null;

            var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-days));
            
            var mealHistory = await _context.Mealplans
                .Include(mp => mp.Meal)
                .Where(mp => mp.Userid == userId && mp.Date >= startDate)
                .ToListAsync();

            if (!mealHistory.Any())
            {
                return new NutritionInsightsDto
                {
                    DaysAnalyzed = days,
                    Insights = new List<string> { "Chưa có dữ liệu để phân tích. Hãy bắt đầu sử dụng thực đơn!" }
                };
            }

            // Group by date for trends
            var dailyData = mealHistory
                .GroupBy(m => m.Date)
                .Select(g => new NutrientTrendDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Calories = g.Sum(m => m.Meal?.Calories ?? 0),
                    Protein = g.Sum(m => m.Meal?.Protein ?? 0),
                    Carbs = g.Sum(m => m.Meal?.Carbs ?? 0),
                    Fat = g.Sum(m => m.Meal?.Fat ?? 0)
                })
                .OrderBy(d => d.Date)
                .ToList();

            var avgCalories = dailyData.Average(d => d.Calories);
            var avgProtein = dailyData.Average(d => d.Protein);
            var avgCarbs = dailyData.Average(d => d.Carbs);
            var avgFat = dailyData.Average(d => d.Fat);

            // Generate insights
            var insights = new List<string>();
            insights.Add($"Trong {days} ngày qua, bạn đã duy trì {dailyData.Count} ngày ăn uống theo kế hoạch.");
            
            if (avgProtein < 50)
                insights.Add("⚠️ Lượng protein trung bình thấp. Cân nhắc tăng thêm thịt, cá, trứng.");
            else
                insights.Add("✅ Lượng protein của bạn ở mức tốt!");

            if (avgCarbs > avgCalories * 0.6m)
                insights.Add("⚠️ Tỷ lệ carbs cao. Cân nhắc giảm tinh bột nếu muốn giảm cân.");
            
            var recommendations = new List<string>
            {
                "Duy trì chế độ ăn đa dạng với nhiều loại rau củ",
                "Uống đủ nước mỗi ngày",
                "Hạn chế đồ ăn chế biến sẵn và fast food",
                "Ăn đủ 3 bữa chính và 1-2 bữa phụ"
            };

            return new NutritionInsightsDto
            {
                DaysAnalyzed = days,
                AverageDailyCalories = avgCalories,
                AverageDailyProtein = avgProtein,
                AverageDailyCarbs = avgCarbs,
                AverageDailyFat = avgFat,
                Trends = dailyData,
                Insights = insights,
                Recommendations = recommendations
            };
        }

        /// <summary>
        /// Gợi ý món ăn theo thời gian trong ngày
        /// </summary>
        public async Task<TimeBasedSuggestionsDto?> GetTimeBasedMealSuggestionsAsync(int userId)
        {
            if (!await IsPremiumUserAsync(userId))
                return null;

            var currentHour = DateTime.Now.Hour;
            string mealTime;
            string reason;

            if (currentHour >= 6 && currentHour < 10)
            {
                mealTime = "Bữa sáng";
                reason = "Bắt đầu ngày mới với bữa sáng đầy năng lượng!";
            }
            else if (currentHour >= 10 && currentHour < 14)
            {
                mealTime = "Bữa trưa";
                reason = "Đã đến giờ bữa trưa! Hãy nạp đủ năng lượng cho buổi chiều.";
            }
            else if (currentHour >= 14 && currentHour < 17)
            {
                mealTime = "Bữa phụ";
                reason = "Thời gian hoàn hảo cho bữa ăn nhẹ!";
            }
            else if (currentHour >= 17 && currentHour < 21)
            {
                mealTime = "Bữa tối";
                reason = "Bữa tối nhẹ nhàng và dinh dưỡng!";
            }
            else
            {
                mealTime = "Bữa phụ";
                reason = "Nếu đói, hãy chọn món ăn nhẹ và lành mạnh.";
            }

            // Get suitable meals
            var calorieRange = mealTime == "Bữa phụ" ? (200, 400) : 
                              mealTime == "Bữa sáng" ? (300, 500) :
                              mealTime == "Bữa trưa" ? (400, 700) :
                              (300, 600);

            var meals = await _context.Meals
                .Where(m => m.StatusId == 1 && 
                           m.Calories >= calorieRange.Item1 && 
                           m.Calories <= calorieRange.Item2)
                .OrderBy(m => Guid.NewGuid())
                .Take(8)
                .Select(m => new MealSuggestionDto
                {
                    MealId = m.Mealid,
                    Name = m.Name,
                    Calories = m.Calories ?? 0,
                    ImageUrl = m.ImageUrl,
                    CookingTime = m.Cookingtime ?? 0,
                    IsQuickPrep = (m.Cookingtime ?? 0) <= 30,
                    WhySuggested = (m.Cookingtime ?? 0) <= 30 
                        ? "Nhanh chóng, dễ làm" 
                        : "Dinh dưỡng cân bằng"
                })
                .ToListAsync();

            return new TimeBasedSuggestionsDto
            {
                CurrentMealTime = mealTime,
                Suggestions = meals,
                ReasonForSuggestions = reason
            };
        }
    }
}
