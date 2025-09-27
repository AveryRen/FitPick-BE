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
            // 1️⃣ Lấy user marks trước (tránh join trực tiếp trong LINQ to Entities)
            var userMarks = await _context.UserMealIngredientMarks
                                          .Where(u => u.Userid == userId)
                                          .ToListAsync();

            // 2️⃣ Lấy mealPlans + meals + mealTimes
            var mealPlansRaw = await (from mp in _context.Mealplans
                                      join m in _context.Meals on mp.Mealid equals m.Mealid
                                      join mt in _context.MealTimes on mp.MealtimeId equals mt.Id
                                      where mp.Userid == userId && mp.Date == DateOnly.FromDateTime(date)
                                      select new
                                      {
                                          MealPlan = mp,
                                          Meal = m,
                                          MealTimeName = mt.Name
                                      }).ToListAsync();

            // 3️⃣ Map thành DTO, load Instructions + Ingredients trong memory
            var result = mealPlansRaw.Select(x => new TodayMealPlanDto
            {
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
                    IsPremium = x.Meal.IsPremium ?? false,
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
                                          .AsEnumerable() // join với userMarks trong memory
                                          .Select(joined =>
                                          {
                                              var mark = userMarks.FirstOrDefault(u =>
                                                  u.Mealid == joined.mi.Mealid &&
                                                  u.Ingredientid == joined.mi.Ingredientid);

                                              return new MealIngredientDto
                                              {
                                                  Name = joined.i.Name,
                                                  Quantity = joined.mi.Quantity ?? 0m,
                                                  Unit = joined.i.Unit,
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

        // Sinh meal plan mới cho 1 ngày, tránh duplicate
        public async Task<List<Mealplan>> GenerateMealPlanAsync(int userId, DateOnly date)
        {
            // Xóa meal plan cũ của user trong ngày (nếu có)
            var existingPlans = await _context.Mealplans
                .Where(mp => mp.Userid == userId && mp.Date == date)
                .ToListAsync();

            if (existingPlans.Any())
                _context.Mealplans.RemoveRange(existingPlans);

            // Lấy user profile
            var profile = await _context.Healthprofiles.FirstOrDefaultAsync(hp => hp.Userid == userId);
            if (profile == null) return null!;

            // Lấy meals phù hợp calories / goal
            var meals = await _context.Meals
                .Where(m => (m.Calories ?? 0) <= (profile.Targetcalories ?? 0))
                .ToListAsync();

            if (!meals.Any()) return null!;

            // Mỗi ngày 3 bữa: sáng, trưa, tối
            var mealTimes = await _context.MealTimes.Take(3).ToListAsync();
            var random = new Random();

            var mealPlans = new List<Mealplan>();

            foreach (var mt in mealTimes)
            {
                // Giả sử mỗi bữa có 2 món ngẫu nhiên (có thể thay đổi số lượng)
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

        // Hoán đổi 1 món
        public async Task<Mealplan?> SwapMealAsync(int planId, int newMealId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null) return null;

            plan.Mealid = newMealId;
            await _context.SaveChangesAsync();
            return plan;
        }

        // Xoá meal plan (1 món)
        public async Task<bool> DeleteMealPlanAsync(int planId)
        {
            var plan = await _context.Mealplans.FindAsync(planId);
            if (plan == null) return false;

            _context.Mealplans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        } 
    }
}