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
                : "kh�ng c�";

        private static string ExtractJson(string input)
        {
            // Tr�ch ra ph?n JSON d?u ti�n (m?ng ho?c object)
            var match = Regex.Match(input, @"(\{.*\}|\[.*\])", RegexOptions.Singleline);
            return match.Success ? match.Value : "";
        }

        /// <summary>
        /// G?i AI, retry v� parse JSON an to�n
        /// </summary>
        public async Task<T> GenerateJsonAsync<T>(string prompt, T defaultValue)
        {
            try
            {
                var policy = Policy
                    .Handle<Exception>() // b?t m?i l?i t? GenerativeAI
                    .WaitAndRetryAsync(
                        3,
                        attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2,4,8s
                        (ex, delay, count, _) =>
                        {
                            _logger.LogWarning(ex,
                                "?? AI th?t b?i (l?n {Retry}). Th? l?i sau {Delay}s",
                                count, delay.TotalSeconds);
                        });

                var response = await policy.ExecuteAsync(() => _model.GenerateContentAsync(prompt));

                var raw = response?.Text?.Trim() ?? "";
                _logger.LogInformation("?? Raw AI output:\n{Raw}", raw);

                var json = ExtractJson(raw);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("? Kh�ng t�m th?y JSON trong output AI");
                    return defaultValue;
                }

                try
                {
                    var result = JsonSerializer.Deserialize<T>(json);
                    return result ?? defaultValue;
                }
                catch (JsonException jex)
                {
                    _logger.LogError(jex, "? Parse JSON th?t b?i");
                    return defaultValue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? AI Service qu� t?i ho?c l?i");
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
B?n l� chuy�n gia dinh du?ng.
�ua ra 3 lo?i nu?c u?ng ph� h?p:
- M?c ti�u: {goal ?? profile.HealthGoal}
- Th?i di?m: {timeOfDay ?? "b?t k?"}
- Tr�nh nguy�n li?u: {avoid}
- Uu ti�n nguy�n li?u: {prefer}
Ch? tr? v? JSON: [""drink1"", ""drink2"", ""drink3""]";

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
T?o th?c don 3 b?a + g?i � nu?c u?ng:
Ng�y: {date:yyyy-MM-dd}
M?c ti�u: {healthGoal ?? profile.HealthGoal}
L?i s?ng: {lifestyle ?? profile.Lifestyle}
Tr�nh nguy�n li?u: {avoid}
Uu ti�n nguy�n li?u: {prefer}
�?nh d?ng JSON:
{{
  ""buasang"": {{""meal"":""..."",""drink"":""...""}},
  ""buatrua"":     {{""meal"":""..."",""drink"":""...""}},
  ""buatoi"":    {{""meal"":""..."",""drink"":""...""}}
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
                return new { error = "Notification type 'Drink' chua t?n t?i" };

            var notification = new Notification
            {
                Userid = userId,
                Title = "Nh?c u?ng nu?c",
                Message = $"Ch�o {profile.FullName}, h�y u?ng 1 c?c nu?c nh�!",
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

            // D�ng d? nh? c�c m�n d� ch?n trong tu?n ? tr�nh tr�ng
            var usedMealIds = new HashSet<int>();

            for (int day = 0; day < 7; day++)
            {
                var avoid = JoinPrefs(MapIdsToNames(avoidIds, allIngredients));
                var prefer = JoinPrefs(MapIdsToNames(preferIds, allIngredients));

                var prompt = $@"
B?n l� chuy�n gia dinh du?ng.
T?o th?c don cho 1 ng�y (3 b?a: s�ng, trua, t?i):
- M?c ti�u: {healthGoal ?? profile.Healthgoal?.Name ?? "any"}
- L?i s?ng: {lifestyle ?? profile.Lifestyle?.Name ?? "any"}
- Tr�nh nguy�n li?u: {avoid}
- Uu ti�n nguy�n li?u: {prefer}
Ch? tr? v? JSON d?ng:
{{
  ""buasang"": ""meal1"",
  ""buatrua"": ""meal2"",
  ""buatoi"": ""meal3""
}}";

                // g?i AI 1 l?n cho c? ng�y
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

                // n?u AI tr? v? h?p l? th� d�ng
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

                // fallback n?u AI fail ho?c kh�ng d? 3 b?a
                if (daily.Meals.Count < 3)
                {
                    // ch?n random nhung lo?i b? m�n d� d�ng
                    var fallbackMeals = allMeals
                        .Where(m => m.CategoryId != 4 && !usedMealIds.Contains(m.Mealid))
                        .OrderBy(x => rnd.Next())
                        .Take(3 - daily.Meals.Count)
                        .ToList();

                    // n?u kh�ng d? th� reset used ? cho ph�p ch?n l?i
                    if (fallbackMeals.Count < (3 - daily.Meals.Count))
                    {
                        usedMealIds.Clear();
                        fallbackMeals = allMeals
                            .Where(m => m.CategoryId != 4)
                            .OrderBy(x => rnd.Next())
                            .Take(3 - daily.Meals.Count)
                            .ToList();
                    }

                    // g�n v�o c�c b?a chua c�
                    var remainingSlots = new List<string> { "buasang", "buatrua", "buatoi" }
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
B?n l� chuy�n gia dinh du?ng.
G?i � 3 m�n an:
- M?c ti�u: {goal ?? profile.HealthGoal}
- Lo?i b?a: {mealType ?? "any"}
- Tr�nh nguy�n li?u: {avoid}
- Uu ti�n nguy�n li?u: {prefer}
Ch? tr? v? JSON: [""meal1"",""meal2"",""meal3""]";

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
