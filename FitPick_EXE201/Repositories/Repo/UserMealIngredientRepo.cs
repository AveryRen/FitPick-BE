using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class UserMealIngredientRepo : IUserMealIngredientRepo
    {
        private readonly FitPickContext _context;

        public UserMealIngredientRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<List<MealIngredientDto>> GetUserMealIngredientsAsync(int userId, int mealId)
        {
            var ingredients = await _context.Mealingredients
                                            .Where(mi => mi.Mealid == mealId)
                                            .Join(_context.Ingredients,
                                                  mi => mi.Ingredientid,
                                                  i => i.Ingredientid,
                                                  (mi, i) => new { mi, i })
                                            .ToListAsync();

            var userMarks = await _context.UserMealIngredientMarks
                                          .Where(u => u.Userid == userId && u.Mealid == mealId)
                                          .ToListAsync();

            return ingredients.Select(x =>
            {
                var mark = userMarks.FirstOrDefault(u => u.Ingredientid == x.mi.Ingredientid);
                return new MealIngredientDto
                {
                    IngredientId = (int)x.mi.Ingredientid,
                    Name = x.i.Name,
                    Quantity = x.mi.Quantity ?? 0m,
                    Unit = x.i.Unit,
                    HasIt = mark?.HasIt ?? false
                };
            }).ToList();
        }
        public async Task MarkIngredientAsync(int userId, int mealId, int ingredientId, bool hasIt)
        {
            var mark = await _context.UserMealIngredientMarks
                                     .FirstOrDefaultAsync(m =>
                                         m.Userid == userId &&
                                         m.Mealid == mealId &&
                                         m.Ingredientid == ingredientId);

            if (mark != null)
            {
                mark.HasIt = hasIt;
            }
            else
            {
                _context.UserMealIngredientMarks.Add(new UserMealIngredientMark
                {
                    Userid = userId,
                    Mealid = mealId,
                    Ingredientid = ingredientId,
                    HasIt = hasIt
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}