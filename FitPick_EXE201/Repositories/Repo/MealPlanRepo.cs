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
            // 1?? L?y user marks tru?c (trnh join tr?c ti?p trong LINQ to Entities)
            var userMarks = await _context.UserMealIngredientMarks
                                          .Where(u => u.Userid == userId)
                                          .ToListAsync();

            // 2. Lấy mealPlans + meals + mealTimes + Category + Status
            var mealPlansRaw = await (from mp in _context.Mealplans
                                      join m in _context.Meals on mp.Mealid equals m.Mealid
                                      join mt in _context.MealTimes on mp.MealtimeId equals mt.Id
                                      join c in _context.MealCategories on m.CategoryId equals c.Id into categoryGroup
                                      from cat in categoryGroup.DefaultIfEmpty()
                                      join s in _context.MealStatuses on m.StatusId equals s.Id into statusGroup
                                      from stat in statusGroup.DefaultIfEmpty()
                                      where mp.Userid == userId && mp.Date == DateOnly.FromDateTime(date)
                                      select new
                                      {
                                          MealPlan = mp,
                                          Meal = m,
                                          MealTimeName = mt.Name,
                                          CategoryName = cat != null ? cat.Name : string.Empty,
                                          StatusName = stat != null ? stat.Name : string.Empty
                                      }).ToListAsync();

            // 3?? Map thnh DTO, load Instructions + Ingredients trong memory
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

        // Sinh meal plan m?i cho 1 ngy, trnh duplicate
        public async Task<List<Mealplan>> GenerateMealPlanAsync(int userId, DateOnly date)
        {
            // Xa meal plan cu c?a user trong ngy (n?u c)
            var existingPlans = await _context.Mealplans
                .Where(mp => mp.Userid == userId && mp.Date == date)
                .ToListAsync();

            if (existingPlans.Any())
                _context.Mealplans.RemoveRange(existingPlans);

            // L?y user profile
            var profile = await _context.Healthprofiles.FirstOrDefaultAsync(hp => hp.Userid == userId);
            if (profile == null)
            {
                return null!;
            }

            // Kiểm tra target calories
            if (!profile.Targetcalories.HasValue || profile.Targetcalories.Value <= 0)
            {
                return null!;
            }

            // L?y meals ph h?p calories / goal
            var meals = await _context.Meals
                .Where(m => m.StatusId == 1 && (m.Calories ?? 0) > 0 && (m.Calories ?? 0) <= profile.Targetcalories.Value)
                .ToListAsync();

            if (meals == null || !meals.Any()) 
            {
                return null!;
            }

            // M?i ngy 3 b?a: sng, trua, t?i
            var mealTimes = await _context.MealTimes.Take(3).ToListAsync();
            if (mealTimes == null || !mealTimes.Any())
            {
                return null!;
            }

            var random = new Random();
            var mealPlans = new List<Mealplan>();

            foreach (var mt in mealTimes)
            {
                if (mt == null) continue;
                
                // Gi? s? m?i b?a c 2 mn ng?u nhin (c th? thay d?i s? lu?ng)
                if (meals == null || !meals.Any()) continue;
                var mealsInTime = meals.OrderBy(x => random.Next()).Take(2).ToList();
                if (mealsInTime == null || !mealsInTime.Any()) continue;
                
                foreach (var meal in mealsInTime)
                {
                    if (meal == null) continue;
                    
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
            
            if (!mealPlans.Any())
            {
                Console.WriteLine($"No meal plans created for user {userId} on date {date}");
                return null!;
            }

            _context.Mealplans.AddRange(mealPlans);
            await _context.SaveChangesAsync();
            return mealPlans;
        }

        // Sinh meal plan mới với target calories được truyền vào (không cần health profile)
        public async Task<List<Mealplan>> GenerateMealPlanWithTargetCaloriesAsync(int userId, DateOnly date, int targetCalories)
        {
            // Xóa meal plan cũ của user trong ngày (nếu có)
            var existingPlans = await _context.Mealplans
                .Where(mp => mp.Userid == userId && mp.Date == date)
                .ToListAsync();

            if (existingPlans.Any())
            {
                _context.Mealplans.RemoveRange(existingPlans);
            }

            // Validate target calories
            if (targetCalories <= 0)
            {
                Console.WriteLine($"❌ Invalid targetCalories: {targetCalories}");
                return null!;
            }

            // Lấy meals phù hợp calories / goal
            var meals = await _context.Meals
                .Where(m => m.StatusId == 1 && (m.Calories ?? 0) > 0 && (m.Calories ?? 0) <= targetCalories)
                .ToListAsync();

            if (meals == null || !meals.Any()) 
            {
                return null!;
            }

            // Mỗi ngày 3 bữa: sáng, trưa, tối
            var mealTimes = await _context.MealTimes.Take(3).ToListAsync();
            if (mealTimes == null || !mealTimes.Any())
            {
                return null!;
            }

            var random = new Random();
            var mealPlans = new List<Mealplan>();

            foreach (var mt in mealTimes)
            {
                if (mt == null) continue;
                
                // Giả sử mỗi bữa có 2 món ngẫu nhiên (có thể thay đổi số lượng)
                if (meals == null || !meals.Any()) continue;
                var mealsInTime = meals.OrderBy(x => random.Next()).Take(2).ToList();
                if (mealsInTime == null || !mealsInTime.Any()) continue;
                
                foreach (var meal in mealsInTime)
                {
                    if (meal == null) continue;
                    
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
            
            if (!mealPlans.Any())
            {
                Console.WriteLine($"No meal plans created for user {userId} on date {date}");
                return null!;
            }

            _context.Mealplans.AddRange(mealPlans);
            await _context.SaveChangesAsync();
            return mealPlans;
        }

        // Hon d?i 1 mn
        public async Task<Mealplan> SwapMealAsync(int planId, int newMealId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null) throw new Exception("Meal plan not found");

            plan.Mealid = newMealId;
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> DeleteMealPlanAsync(int planId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null) return false;

            _context.Mealplans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        // Thay đổi món từ gợi ý
        public async Task<Mealplan?> ReplaceMealBySuggestionAsync(int planId, int userId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null || plan.Userid != userId) return null;

            // Lấy thông tin món ăn hiện tại để lấy tag
            var currentMeal = await _context.Meals.FindAsync(plan.Mealid);
            if (currentMeal == null)
            {
                Console.WriteLine("❌ Current meal not found");
                return null;
            }

            // Strategy 1: Tìm món cùng category và cùng loại (premium/free)
            var sameCategoryMeal = await _context.Meals
                .Where(m => m.CategoryId == currentMeal.CategoryId && 
                           m.IsPremium == currentMeal.IsPremium && 
                           m.Mealid != plan.Mealid && 
                           m.StatusId == 1)
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (sameCategoryMeal != null)
            {
                plan.Mealid = sameCategoryMeal.Mealid;
                await _context.SaveChangesAsync();
                return plan;
            }

            // Strategy 2: Tìm bất kỳ món miễn phí nào khác
            var anyFreeMeal = await _context.Meals
                .Where(m => m.IsPremium == false && m.Mealid != plan.Mealid && m.StatusId == 1)
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (anyFreeMeal != null)
            {
                plan.Mealid = anyFreeMeal.Mealid;
                await _context.SaveChangesAsync();
                return plan;
            }

            // Strategy 3: Tìm bất kỳ món premium nào khác
            var premiumMeal = await _context.Meals
                .Where(m => m.Mealid != plan.Mealid && m.StatusId == 1)
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (premiumMeal != null)
            {
                plan.Mealid = premiumMeal.Mealid;
                await _context.SaveChangesAsync();
                return plan;
            }

            // Strategy 4: Nếu chỉ có 1 món trong database, tạo món mới hoặc trả về lỗi
            Console.WriteLine($"❌ All strategies failed! No other meals found in database.");
            
            // Trả về null để frontend biết là lỗi
            return null;
        }

        // Thay đổi món từ danh sách yêu thích
        public async Task<Mealplan?> ReplaceMealByFavoritesAsync(int planId, int userId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null || plan.Userid != userId) return null;

            // Lấy thông tin món ăn hiện tại
            var currentMeal = await _context.Meals.FindAsync(plan.Mealid);
            if (currentMeal == null)
            {
                Console.WriteLine("❌ Current meal not found");
                return null;
            }

            // Lấy danh sách món yêu thích của user
            var favoriteMealIds = await _context.MealFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.MealId)
                .ToListAsync();

            if (!favoriteMealIds.Any())
            {
                Console.WriteLine("❌ User has no favorite meals");
                return null;
            }

            // Strategy 1: Tìm món yêu thích cùng category và cùng loại (premium/free)
            var sameCategoryFavorite = await _context.Meals
                .Where(m => favoriteMealIds.Contains(m.Mealid) &&
                           m.CategoryId == currentMeal.CategoryId &&
                           m.IsPremium == currentMeal.IsPremium &&
                           m.Mealid != plan.Mealid &&
                           m.StatusId == 1)
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (sameCategoryFavorite != null)
            {
                plan.Mealid = sameCategoryFavorite.Mealid;
                await _context.SaveChangesAsync();
                return plan;
            }

            // Strategy 2: Tìm bất kỳ món yêu thích miễn phí nào khác
            var anyFreeFavorite = await _context.Meals
                .Where(m => favoriteMealIds.Contains(m.Mealid) &&
                           m.IsPremium == false &&
                           m.Mealid != plan.Mealid &&
                           m.StatusId == 1)
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (anyFreeFavorite != null)
            {
                plan.Mealid = anyFreeFavorite.Mealid;
                await _context.SaveChangesAsync();
                return plan;
            }

            // Strategy 3: Tìm bất kỳ món yêu thích premium nào khác
            var premiumFavorite = await _context.Meals
                .Where(m => favoriteMealIds.Contains(m.Mealid) &&
                           m.Mealid != plan.Mealid &&
                           m.StatusId == 1)
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (premiumFavorite != null)
            {
                plan.Mealid = premiumFavorite.Mealid;
                await _context.SaveChangesAsync();
                return plan;
            }

            Console.WriteLine($"❌ All strategies failed! No favorite meals found to replace.");
            return null;
        }

        // Thêm món vào menu
        public async Task<Mealplan?> AddMealToMenuAsync(int userId, int mealId, DateTime date, string? mealTime)
        {
            var dateOnly = DateOnly.FromDateTime(date);
            
            // Tìm meal time từ tên
            int? mealTimeId = null;
            if (!string.IsNullOrEmpty(mealTime))
            {
                var mt = await _context.MealTimes
                    .FirstOrDefaultAsync(m => m.Name.ToLower() == mealTime.ToLower());
                mealTimeId = mt?.Id;
            }

            // Nếu không tìm thấy meal time, lấy mặc định (bữa sáng)
            if (!mealTimeId.HasValue)
            {
                var defaultMealTime = await _context.MealTimes.FirstOrDefaultAsync();
                mealTimeId = defaultMealTime?.Id;
            }

            if (!mealTimeId.HasValue)
            {
                Console.WriteLine("❌ Debug - No meal time found");
                return null;
            }

            var mealPlan = new Mealplan
            {
                Userid = userId,
                Date = dateOnly,
                MealtimeId = mealTimeId.Value,
                Mealid = mealId,
                StatusId = 1 // default
            };

            _context.Mealplans.Add(mealPlan);
            await _context.SaveChangesAsync();
            return mealPlan;
        }
    }
}
