using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
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
    }
}
