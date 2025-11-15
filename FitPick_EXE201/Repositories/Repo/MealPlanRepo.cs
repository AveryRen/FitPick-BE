using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class MealPlanRepo : IMealPlanRepo
    {
        private readonly FitPickContext _context;

        public MealPlanRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<List<TodayMealPlanDto>> GetTodayMealPlanAsync(int userId, DateTime date)
        {
            // 1?? L?y user marks tru?c (tr�nh join tr?c ti?p trong LINQ to Entities)
            var userMarks = await _context.UserMealIngredientMarks
                                          .Where(u => u.Userid == userId)
                                          .ToListAsync();

            // 2. Lấy mealPlans + meals + mealTimes + Category + Status
            var mealPlansRaw = await (from mp in _context.Mealplans
                                      join m in _context.Meals on mp.Mealid equals m.Mealid
                                      join mt in _context.MealTimes on mp.MealtimeId equals mt.Id
                                      join c in _context.MealCategories on m.CategoryId equals c.Id into categoryGroup
                                      from category in categoryGroup.DefaultIfEmpty()
                                      join s in _context.MealStatuses on m.StatusId equals s.Id into statusGroup
                                      from status in statusGroup.DefaultIfEmpty()
                                      where mp.Userid == userId && mp.Date == DateOnly.FromDateTime(date)
                                      select new
                                      {
                                          MealPlan = mp,
                                          Meal = m,
                                          MealTimeName = mt.Name,
                                          CategoryName = category != null ? category.Name : null,
                                          StatusName = status != null ? status.Name : null
                                      }).ToListAsync();

            // 3?? Map th�nh DTO, load Instructions + Ingredients trong memory
            var result = mealPlansRaw.Select(x => new TodayMealPlanDto
            {
                PlanId = x.MealPlan.Planid,
                Date = x.MealPlan.Date.ToDateTime(TimeOnly.MinValue),
                MealTime = x.MealTimeName,
                Meal = new MealDto
                {
                    Mealid = x.Meal.Mealid,
                    Name = x.Meal.Name,
                    Description = x.Meal.Description,
                    Calories = x.Meal.Calories ?? 0,
                    Protein = x.Meal.Protein ?? 0,
                    Carbs = x.Meal.Carbs ?? 0,
                    Fat = x.Meal.Fat ?? 0,
                    Cookingtime = x.Meal.Cookingtime ?? 0,
                    Diettype = x.Meal.Diettype,
                    Price = x.Meal.Price,
                    ImageUrl = x.Meal.ImageUrl,
                    IsPremium = x.Meal.IsPremium ?? false,
                    CategoryName = x.CategoryName,
                    StatusName = x.StatusName,
                    Instructions = _context.MealInstructions
                                           .Where(mi => mi.MealId == x.Meal.Mealid)
                                           .OrderBy(mi => mi.StepNumber)
                                           .Select(mi => mi.Instruction)
                                           .ToList(),
                    Ingredients = _context.Mealingredients
                                          .Where(mi => mi.Mealid == x.Meal.Mealid)
                                          .Join(_context.Ingredients,
                                                mi => mi.Ingredientid,
                                                i => i.Ingredientid,
                                                (mi, i) => new { mi, i })
                                          .AsEnumerable() // join v?i userMarks trong memory
                                          .Select(joined =>
                                          {
                                              var mark = userMarks.FirstOrDefault(u =>
                                                  u.Mealid == joined.mi.Mealid &&
                                                  u.Ingredientid == joined.mi.Ingredientid);

                                              return new MealIngredientDto
                                              {
                                                  Name = joined.i.Name ?? string.Empty,
                                                  Quantity = joined.mi.Quantity ?? 0m,
                                                  Unit = joined.i.Unit ?? string.Empty,
                                                  HasIt = mark?.HasIt ?? false
                                              };
                                          }).ToList()
                }
            }).ToList();

            return result;
        }
        public async Task<List<Mealplan>> GetUserMealPlansAsync(int userId)
        {
            return await _context.Mealplans
                .Include(mp => mp.Meal)
                .Include(mp => mp.Mealtime)
                .Where(mp => mp.Userid == userId)
                .OrderBy(mp => mp.Date)
                .ThenBy(mp => mp.MealtimeId)
                .ToListAsync();
        }

        // Sinh meal plan m?i cho 1 ng�y, tr�nh duplicate
        public async Task<List<Mealplan>> GenerateMealPlanAsync(int userId, DateOnly date)
        {
            // X�a meal plan cu c?a user trong ng�y (n?u c�)
            var existingPlans = await _context.Mealplans
                .Where(mp => mp.Userid == userId && mp.Date == date)
                .ToListAsync();

            if (existingPlans.Any())
                _context.Mealplans.RemoveRange(existingPlans);

            // L?y user profile
            var profile = await _context.Healthprofiles.FirstOrDefaultAsync(hp => hp.Userid == userId);
            if (profile == null) return null!;

            // L?y meals ph� h?p calories / goal
            var meals = await _context.Meals
                .Where(m => (m.Calories ?? 0) <= (profile.Targetcalories ?? 0))
                .ToListAsync();

            if (!meals.Any()) return null!;

            // M?i ng�y 3 b?a: s�ng, trua, t?i
            var mealTimes = await _context.MealTimes.Take(3).ToListAsync();
            var random = new Random();

            var mealPlans = new List<Mealplan>();

            foreach (var mt in mealTimes)
            {
                // Gi? s? m?i b?a c� 2 m�n ng?u nhi�n (c� th? thay d?i s? lu?ng)
                var mealsInTime = meals.OrderBy(x => random.Next()).Take(2).ToList();
                foreach (var meal in mealsInTime)
                {
                    mealPlans.Add(new Mealplan
                    {
                        Userid = userId,
                        Date = date,
                        MealtimeId = mt.Id,
                        Mealid = meal.Mealid,
                        StatusId = 1 // default
                    });
                }
            }

            _context.Mealplans.AddRange(mealPlans);
            await _context.SaveChangesAsync();
            return mealPlans;
        }

        // Ho�n d?i 1 m�n
        public async Task<Mealplan?> SwapMealAsync(int planId, int newMealId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null) return null;

            plan.Mealid = newMealId;
            await _context.SaveChangesAsync();
            return plan;
        }

        // Xo� meal plan (1 m�n)
        public async Task<bool> DeleteMealPlanAsync(int planId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null) return false;

            _context.Mealplans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        // Thay đổi món theo gợi ý
        public async Task<Mealplan?> ReplaceMealBySuggestionAsync(int planId, int userId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null || plan.Userid != userId) return null;

            Console.WriteLine($"🔄 Debug - ReplaceMealBySuggestion: planId={planId}, userId={userId}");
            Console.WriteLine($"🔄 Debug - Current mealId: {plan.Mealid}");

            // Lấy thông tin món ăn hiện tại để lấy tag
            var currentMeal = await _context.Meals
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Mealid == plan.Mealid);

            if (currentMeal == null) return null;

            Console.WriteLine($"🔄 Debug - Current meal: {currentMeal.Name}, CategoryId: {currentMeal.CategoryId}, Diettype: {currentMeal.Diettype}");

            // Strategy 1: Tìm món miễn phí phù hợp với tag
            var suggestedMeal = await _context.Meals
                .Where(m => m.IsPremium == false 
                    && m.Mealid != plan.Mealid 
                    && m.StatusId == 1
                    && (m.CategoryId == currentMeal.CategoryId || m.Diettype == currentMeal.Diettype))
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();

            Console.WriteLine($"🔄 Debug - Strategy 1 - Found suggested meal: {(suggestedMeal?.Name ?? "None")}");

            if (suggestedMeal != null)
            {
                var oldMealId = plan.Mealid;
                plan.Mealid = suggestedMeal.Mealid;
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Debug - Strategy 1 SUCCESS: Replaced meal {oldMealId} with {suggestedMeal.Mealid} ({suggestedMeal.Name})");
                return plan;
            }

            // Strategy 2: Tìm bất kỳ món miễn phí nào khác
            Console.WriteLine($"⚠️ Debug - Strategy 1 failed, trying Strategy 2...");
            var anyFreeMeal = await _context.Meals
                .Where(m => m.IsPremium == false && m.Mealid != plan.Mealid && m.StatusId == 1)
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();

            Console.WriteLine($"🔄 Debug - Strategy 2 - Found any free meal: {(anyFreeMeal?.Name ?? "None")}");

            if (anyFreeMeal != null)
            {
                var oldMealId = plan.Mealid;
                plan.Mealid = anyFreeMeal.Mealid;
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Debug - Strategy 2 SUCCESS: Replaced meal {oldMealId} with {anyFreeMeal.Mealid} ({anyFreeMeal.Name})");
                return plan;
            }

            // Strategy 3: Tìm bất kỳ món premium nào khác
            Console.WriteLine($"⚠️ Debug - Strategy 2 failed, trying Strategy 3...");
            var premiumMeal = await _context.Meals
                .Where(m => m.Mealid != plan.Mealid && m.StatusId == 1)
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();

            Console.WriteLine($"🔄 Debug - Strategy 3 - Found premium meal: {(premiumMeal?.Name ?? "None")}");

            if (premiumMeal != null)
            {
                var oldMealId = plan.Mealid;
                plan.Mealid = premiumMeal.Mealid;
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Debug - Strategy 3 SUCCESS: Replaced meal {oldMealId} with {premiumMeal.Mealid} ({premiumMeal.Name})");
                return plan;
            }

            // Strategy 4: Nếu chỉ có 1 món trong database, tạo món mới hoặc trả về lỗi
            Console.WriteLine($"❌ Debug - All strategies failed! No other meals found in database.");
            Console.WriteLine($"❌ Debug - Total meals in database: {await _context.Meals.CountAsync()}");
            Console.WriteLine($"❌ Debug - Active meals: {await _context.Meals.Where(m => m.StatusId == 2).CountAsync()}");
            
            // Trả về null để frontend biết là lỗi
            return null;
        }

        // Thay đổi món từ danh sách yêu thích
        public async Task<Mealplan?> ReplaceMealByFavoritesAsync(int planId, int userId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null || plan.Userid != userId) return null;

            Console.WriteLine($"🔄 Debug - ReplaceMealByFavorites: planId={planId}, userId={userId}");
            Console.WriteLine($"🔄 Debug - Current mealId: {plan.Mealid}");

            // Lấy thông tin món ăn hiện tại để lấy tag
            var currentMeal = await _context.Meals
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Mealid == plan.Mealid);

            if (currentMeal == null) return null;

            Console.WriteLine($"🔄 Debug - Current meal: {currentMeal.Name}, CategoryId: {currentMeal.CategoryId}, Diettype: {currentMeal.Diettype}");

            // Strategy 1: Tìm món yêu thích phù hợp với tag
            var favoriteMeal = await (from fm in _context.MealFavorites
                                    join m in _context.Meals on fm.MealId equals m.Mealid
                                    where fm.UserId == userId 
                                        && m.Mealid != plan.Mealid
                                        && m.StatusId == 1
                                        && (m.CategoryId == currentMeal.CategoryId || m.Diettype == currentMeal.Diettype)
                                    orderby Guid.NewGuid()
                                    select m)
                                    .FirstOrDefaultAsync();

            Console.WriteLine($"🔄 Debug - Strategy 1 - Found favorite meal: {(favoriteMeal?.Name ?? "None")}");

            if (favoriteMeal != null)
            {
                var oldMealId = plan.Mealid;
                plan.Mealid = favoriteMeal.Mealid;
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Debug - Strategy 1 SUCCESS: Replaced meal {oldMealId} with {favoriteMeal.Mealid} ({favoriteMeal.Name})");
                return plan;
            }

            // Strategy 2: Tìm bất kỳ món yêu thích nào khác
            Console.WriteLine($"⚠️ Debug - Strategy 1 failed, trying Strategy 2...");
            var anyFavoriteMeal = await (from fm in _context.MealFavorites
                                       join m in _context.Meals on fm.MealId equals m.Mealid
                                       where fm.UserId == userId 
                                           && m.Mealid != plan.Mealid
                                           && m.StatusId == 1
                                       orderby Guid.NewGuid()
                                       select m)
                                       .FirstOrDefaultAsync();

            Console.WriteLine($"🔄 Debug - Strategy 2 - Found any favorite meal: {(anyFavoriteMeal?.Name ?? "None")}");

            if (anyFavoriteMeal != null)
            {
                var oldMealId = plan.Mealid;
                plan.Mealid = anyFavoriteMeal.Mealid;
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Debug - Strategy 2 SUCCESS: Replaced meal {oldMealId} with {anyFavoriteMeal.Mealid} ({anyFavoriteMeal.Name})");
                return plan;
            }

            // Strategy 3: Fallback to suggestion if no favorites
            Console.WriteLine($"⚠️ Debug - No favorites found, falling back to suggestion...");
            return await ReplaceMealBySuggestionAsync(planId, userId);
        }

        // Thêm món ăn vào thực đơn
        public async Task<Mealplan?> AddMealToMenuAsync(int userId, int mealId, DateTime date, string? mealTime)
        {
            try
            {
                // Lấy meal time ID (default là breakfast nếu không chỉ định)
                int mealTimeId = 1; // Default breakfast
                if (!string.IsNullOrEmpty(mealTime))
                {
                    var mealTimeEntity = await _context.MealTimes
                        .FirstOrDefaultAsync(mt => mt.Name.ToLower() == mealTime.ToLower());
                    if (mealTimeEntity != null)
                        mealTimeId = mealTimeEntity.Id;
                }

                // Kiểm tra xem món ăn đã tồn tại trong thực đơn chưa
                var existingPlan = await _context.Mealplans
                    .FirstOrDefaultAsync(mp => mp.Userid == userId 
                        && mp.Date == DateOnly.FromDateTime(date) 
                        && mp.MealtimeId == mealTimeId 
                        && mp.Mealid == mealId);

                if (existingPlan != null)
                {
                    // Món ăn đã tồn tại, trả về plan hiện tại
                    return existingPlan;
                }

                // Tạo meal plan mới
                var newMealPlan = new Mealplan
                {
                    Userid = userId,
                    Date = DateOnly.FromDateTime(date),
                    MealtimeId = mealTimeId,
                    Mealid = mealId,
                    StatusId = 1 // Active
                };

                _context.Mealplans.Add(newMealPlan);
                await _context.SaveChangesAsync();

                return newMealPlan;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding meal to menu: {ex.Message}");
                return null;
            }
        }
    }
}
