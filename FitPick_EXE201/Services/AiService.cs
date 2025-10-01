using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using GenerativeAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FitPick_EXE201.Services
{
    public class AiService
    {
        private readonly GenerativeModel _model;
        private readonly IUserRepo _userRepo;
        private readonly IAIIngredientRepo _ingredientRepo;
        private readonly FitPickContext _context;
        private readonly ILogger<AiService> _logger;

        public AiService(
            IConfiguration cfg,
            IUserRepo userRepo,
            IAIIngredientRepo ingredientRepo,
            FitPickContext context,
            ILogger<AiService> logger)
        {
            _userRepo = userRepo;
            _ingredientRepo = ingredientRepo;
            _context = context;
            _logger = logger;

            _model = new GenerativeModel(
                model: "gemini-2.5-flash",
                apiKey: cfg["Gemini:ApiKey"]
            );
        }

        // ================== Helper ==================
        private static string JoinPrefs(List<string>? prefs)
            => (prefs != null && prefs.Count > 0)
                ? string.Join(", ", prefs)
                : "không có";

        private static string ExtractJson(string input)
        {
            // Trích ra phần JSON đầu tiên (mảng hoặc object)
            var match = Regex.Match(input, @"(\{.*\}|\[.*\])", RegexOptions.Singleline);
            return match.Success ? match.Value : "";
        }

        /// <summary>
        /// Gọi AI, retry và parse JSON an toàn
        /// </summary>
        public async Task<T> GenerateJsonAsync<T>(string prompt, T defaultValue)
        {
            try
            {
                var policy = Policy
                    .Handle<Exception>() // bắt mọi lỗi từ GenerativeAI
                    .WaitAndRetryAsync(
                        3,
                        attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2,4,8s
                        (ex, delay, count, _) =>
                        {
                            _logger.LogWarning(ex,
                                "⚠️ AI thất bại (lần {Retry}). Thử lại sau {Delay}s",
                                count, delay.TotalSeconds);
                        });

                var response = await policy.ExecuteAsync(() => _model.GenerateContentAsync(prompt));

                var raw = response?.Text?.Trim() ?? "";
                _logger.LogInformation("🔎 Raw AI output:\n{Raw}", raw);

                var json = ExtractJson(raw);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("❌ Không tìm thấy JSON trong output AI");
                    return defaultValue;
                }

                try
                {
                    var result = JsonSerializer.Deserialize<T>(json);
                    return result ?? defaultValue;
                }
                catch (JsonException jex)
                {
                    _logger.LogError(jex, "❌ Parse JSON thất bại");
                    return defaultValue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AI Service quá tải hoặc lỗi");
                return defaultValue;
            }
        }

        private async Task<UserAIProfileDto?> GetProfile(int userId)
            => await _userRepo.GetUserAIProfileAsync(userId);

        private static List<string> MapIdsToNames(List<int>? ids, List<Ingredient> allIngredients)
        {
            if (ids == null || ids.Count == 0) return new List<string>();
            return allIngredients
                .Where(i => ids.Contains(i.Ingredientid))
                .Select(i => i.Name)
                .ToList();
        }

        // ================== AI APIs ==================

        public async Task<object> GetDrinkRecommendation(int userId, string? timeOfDay, string? goal)
        {
            var profile = await GetProfile(userId);
            if (profile == null) return new { error = "User not found" };

            var allIngredients = await _ingredientRepo.GetAllIngredientsAsync();
            var avoid = JoinPrefs(MapIdsToNames(profile.Allergies, allIngredients)
                .Concat(MapIdsToNames(profile.Religiondiet, allIngredients)).ToList());
            var prefer = JoinPrefs(MapIdsToNames(profile.Dietarypreferences, allIngredients));

            var prompt = $@"
Bạn là chuyên gia dinh dưỡng.
Đưa ra 3 loại nước uống phù hợp:
- Mục tiêu: {goal ?? profile.HealthGoal}
- Thời điểm: {timeOfDay ?? "bất kỳ"}
- Tránh nguyên liệu: {avoid}
- Ưu tiên nguyên liệu: {prefer}
Chỉ trả về JSON: [""drink1"", ""drink2"", ""drink3""]";

            var aiResult = await GenerateJsonAsync(prompt, new List<string>());

            if (aiResult == null || !aiResult.Any())
            {
                var drinks = await _ingredientRepo.GetDrinksFromMealsAsync();
                return drinks.Take(3).Select(d => d.Name).ToList();
            }
            return aiResult;
        }

        public async Task<object> GenerateMealPlan(
            int userId, DateTime date,
            string? healthGoal, string? lifestyle)
        {
            var profile = await GetProfile(userId);
            if (profile == null) return new { error = "User not found" };

            var allIngredients = await _ingredientRepo.GetAllIngredientsAsync();
            var avoid = JoinPrefs(MapIdsToNames(profile.Allergies, allIngredients)
                .Concat(MapIdsToNames(profile.Religiondiet, allIngredients)).ToList());
            var prefer = JoinPrefs(MapIdsToNames(profile.Dietarypreferences, allIngredients));

            var prompt = $@"
Tạo thực đơn 3 bữa + gợi ý nước uống:
Ngày: {date:yyyy-MM-dd}
Mục tiêu: {healthGoal ?? profile.HealthGoal}
Lối sống: {lifestyle ?? profile.Lifestyle}
Tránh nguyên liệu: {avoid}
Ưu tiên nguyên liệu: {prefer}
Định dạng JSON:
{{
  ""breakfast"": {{""meal"":""..."",""drink"":""...""}},
  ""lunch"":     {{""meal"":""..."",""drink"":""...""}},
  ""dinner"":    {{""meal"":""..."",""drink"":""...""}}
}}";

            return await GenerateJsonAsync(prompt, new object());
        }

        public async Task<object> GenerateDrinkNotification(int userId)
        {
            var profile = await GetProfile(userId);
            if (profile == null)
                return new { error = "User not found" };

            var type = await _context.NotificationTypes
                                     .FirstOrDefaultAsync(t => t.Name == "Drink");
            if (type == null)
                return new { error = "Notification type 'Drink' chưa tồn tại" };

            var notification = new Notification
            {
                Userid = userId,
                Title = "Nhắc uống nước",
                Message = $"Chào {profile.FullName}, hãy uống 1 cốc nước nhé!",
                TypeId = type.Id,
                Isread = false,
                Createdat = DateTime.Now,
                Scheduledat = DateTime.Now,
                IsDone = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return new { notification = notification.Message };
        }

        public async Task<List<DailyMealPlanDto>> GenerateWeeklyMealPlanWithAI(
            int userId, string? healthGoal, string? lifestyle)
        {
            var profile = await _context.Healthprofiles
                .Include(h => h.User)
                .Include(h => h.Healthgoal)
                .Include(h => h.Lifestyle)
                .FirstOrDefaultAsync(h => h.Userid == userId && h.Status == true);

            if (profile == null) return new List<DailyMealPlanDto>();

            var avoidIds = profile.Allergies ?? new List<int>();
            var preferIds = profile.Chronicdiseases ?? new List<int>();
            var allMeals = await _ingredientRepo.GetMealsAsync();
            var allIngredients = await _ingredientRepo.GetAllIngredientsAsync();

            var weekPlan = new List<DailyMealPlanDto>();
            var rnd = new Random();

            // Dùng để nhớ các món đã chọn trong tuần → tránh trùng
            var usedMealIds = new HashSet<int>();

            for (int day = 0; day < 7; day++)
            {
                var avoid = JoinPrefs(MapIdsToNames(avoidIds, allIngredients));
                var prefer = JoinPrefs(MapIdsToNames(preferIds, allIngredients));

                var prompt = $@"
Bạn là chuyên gia dinh dưỡng.
Tạo thực đơn cho 1 ngày (3 bữa: sáng, trưa, tối):
- Mục tiêu: {healthGoal ?? profile.Healthgoal?.Name ?? "any"}
- Lối sống: {lifestyle ?? profile.Lifestyle?.Name ?? "any"}
- Tránh nguyên liệu: {avoid}
- Ưu tiên nguyên liệu: {prefer}
Chỉ trả về JSON dạng:
{{
  ""breakfast"": ""meal1"",
  ""lunch"": ""meal2"",
  ""dinner"": ""meal3""
}}";

                // gọi AI 1 lần cho cả ngày
                var aiJson = await GenerateJsonAsync(prompt, new List<string>());

                Dictionary<string, string>? aiDayMeals = null;
                if (aiJson != null && aiJson.Any())
                {
                    try
                    {
                        aiDayMeals = JsonSerializer.Deserialize<Dictionary<string, string>>(aiJson.First());
                    }
                    catch
                    {
                        aiDayMeals = null;
                    }
                }

                var daily = new DailyMealPlanDto
                {
                    Date = DateTime.Today.AddDays(day),
                    Meals = new Dictionary<string, Meal>()
                };

                // nếu AI trả về hợp lệ thì dùng
                if (aiDayMeals != null)
                {
                    foreach (var kv in aiDayMeals)
                    {
                        var selected = allMeals.FirstOrDefault(m => m.Name == kv.Value);
                        if (selected != null && !usedMealIds.Contains(selected.Mealid))
                        {
                            daily.Meals[kv.Key] = selected;
                            usedMealIds.Add(selected.Mealid);
                        }
                    }
                }

                // fallback nếu AI fail hoặc không đủ 3 bữa
                if (daily.Meals.Count < 3)
                {
                    // chọn random nhưng loại bỏ món đã dùng
                    var fallbackMeals = allMeals
                        .Where(m => m.CategoryId != 4 && !usedMealIds.Contains(m.Mealid))
                        .OrderBy(x => rnd.Next())
                        .Take(3 - daily.Meals.Count)
                        .ToList();

                    // nếu không đủ thì reset used → cho phép chọn lại
                    if (fallbackMeals.Count < (3 - daily.Meals.Count))
                    {
                        usedMealIds.Clear();
                        fallbackMeals = allMeals
                            .Where(m => m.CategoryId != 4)
                            .OrderBy(x => rnd.Next())
                            .Take(3 - daily.Meals.Count)
                            .ToList();
                    }

                    // gán vào các bữa chưa có
                    var remainingSlots = new List<string> { "breakfast", "lunch", "dinner" }
                        .Where(slot => !daily.Meals.ContainsKey(slot))
                        .ToList();

                    for (int i = 0; i < fallbackMeals.Count && i < remainingSlots.Count; i++)
                    {
                        daily.Meals[remainingSlots[i]] = fallbackMeals[i];
                        usedMealIds.Add(fallbackMeals[i].Mealid);
                    }
                }

                weekPlan.Add(daily);
            }

            return weekPlan;
        }


        public async Task<object> GetMealRecommendation(int userId, string? mealType, string? goal)
        {
            var profile = await GetProfile(userId);
            if (profile == null) return new { error = "User not found" };

            var allIngredients = await _ingredientRepo.GetAllIngredientsAsync();
            var avoid = JoinPrefs(MapIdsToNames(profile.Allergies, allIngredients)
                .Concat(MapIdsToNames(profile.Religiondiet, allIngredients)).ToList());
            var prefer = JoinPrefs(MapIdsToNames(profile.Dietarypreferences, allIngredients));

            var prompt = $@"
Bạn là chuyên gia dinh dưỡng.
Gợi ý 3 món ăn:
- Mục tiêu: {goal ?? profile.HealthGoal}
- Loại bữa: {mealType ?? "any"}
- Tránh nguyên liệu: {avoid}
- Ưu tiên nguyên liệu: {prefer}
Chỉ trả về JSON: [""meal1"",""meal2"",""meal3""]";

            var aiResult = await GenerateJsonAsync(prompt, new List<string>());

            if (aiResult == null || !aiResult.Any())
            {
                var meals = await _ingredientRepo.GetMealsAsync();
                return meals.Take(3).Select(m => m.Name).ToList();
            }
            return aiResult;
        }
    }

    // DTO
    public class DailyMealPlanDto
    {
        public DateTime Date { get; set; }
        public Dictionary<string, Meal> Meals { get; set; } = new();
    }
}
