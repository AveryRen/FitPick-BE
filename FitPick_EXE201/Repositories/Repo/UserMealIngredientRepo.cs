using System;
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

            // Tự động tạo records cho các nguyên liệu chưa có mark
            var marksToCreate = new List<UserMealIngredientMark>();
            foreach (var ingredient in ingredients)
            {
                if (!ingredient.mi.Ingredientid.HasValue)
                {
                    throw new InvalidOperationException("Ingredient must have an id before creating marks.");
                }

                var ingredientId = ingredient.mi.Ingredientid.Value;
                var existingMark = userMarks.FirstOrDefault(u => u.Ingredientid == ingredientId);
                if (existingMark == null)
                {
                    // Tạo mark mới với hasIt = false (mặc định)
                    marksToCreate.Add(new UserMealIngredientMark
                    {
                        Userid = userId,
                        Mealid = mealId,
                        Ingredientid = ingredientId,
                        HasIt = false
                    });
                }
            }

            // Thêm tất cả marks mới vào database cùng lúc
            if (marksToCreate.Any())
            {
                _context.UserMealIngredientMarks.AddRange(marksToCreate);
                await _context.SaveChangesAsync();
                // Reload userMarks sau khi tạo mới
                userMarks = await _context.UserMealIngredientMarks
                                          .Where(u => u.Userid == userId && u.Mealid == mealId)
                                          .ToListAsync();
            }

            return ingredients.Select(x =>
            {
                var ingredientId = x.mi.Ingredientid
                    ?? throw new InvalidOperationException("Ingredient must have an id.");

                var mark = userMarks.FirstOrDefault(u => u.Ingredientid == ingredientId);
                return new MealIngredientDto
                {
                    IngredientId = ingredientId,
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
