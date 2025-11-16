using Microsoft.EntityFrameworkCore;
using FitPick_EXE201.Data;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Repositories.Repo
{
    public class FilterRepository : IFilterRepository
    {
        private readonly FitPickContext _context;

        public FilterRepository(FitPickContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetCategoriesAsync()
        {
            var categories = await _context.MealCategories
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    vietnameseName = c.Name
                })
                .ToListAsync();

            return categories.Cast<object>().ToList();
        }

        public async Task<List<object>> GetMealStatusesAsync()
        {
            var statuses = await _context.MealStatuses
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    vietnameseName = s.Name
                })
                .ToListAsync();

            return statuses.Cast<object>().ToList();
        }

        public async Task<List<object>> GetIngredientsAsync(int page = 0, int pageSize = 20)
        {
            var ingredients = await _context.Ingredients
                .Where(i => i.Status == true) // Active ingredients only
                .Select(i => new
                {
                    id = i.Ingredientid,
                    name = i.Name,
                    vietnameseName = i.Name
                })
                .OrderBy(i => i.name)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return ingredients.Cast<object>().ToList();
        }

        public async Task<List<object>> GetDietTypesAsync()
        {
            // Get distinct diet types from Meals table (not from user's diet plans)
            var dietTypes = await _context.Meals
                .Where(m => !string.IsNullOrEmpty(m.Diettype))
                .Select(m => m.Diettype)
                .Distinct()
                .OrderBy(dt => dt)
                .Select(dt => new
                {
                    name = dt,
                    vietnameseName = dt
                })
                .ToListAsync(); 
            return dietTypes.Cast<object>().ToList();
        }

        public Task<List<object>> GetCookingTimesAsync()
        {
            // Create predefined ranges
            var timeRanges = new List<object>
            {
                new { id = "≤ 15 phút", name = "≤ 15 phút", maxMinutes = 15 },
                new { id = "≤ 30 phút", name = "≤ 30 phút", maxMinutes = 30 },
                new { id = "≤ 60 phút", name = "≤ 60 phút", maxMinutes = 60 }
            };

            return Task.FromResult(timeRanges);
        }

        public Task<List<object>> GetMealTypesAsync()
        {
            // These are predefined meal types
            var mealTypes = new List<object>
            {
                new { id = "breakfast", name = "Bữa sáng", englishName = "Breakfast" },
                new { id = "lunch", name = "Bữa trưa", englishName = "Lunch" },
                new { id = "dinner", name = "Bữa tối", englishName = "Dinner" }
            };

            return Task.FromResult(mealTypes);
        }

        public async Task<List<object>> GetUserDietPlansAsync(int userId)
        {
            // Get user's specific diet plan
            var userDietPlan = await _context.Users
                .Where(u => u.Userid == userId)
                .Include(u => u.DietPlan)
                .Select(u => new
                {
                    id = u.DietPlan.Id,
                    name = u.DietPlan.Name,
                    vietnameseName = u.DietPlan.Name,
                    description = u.DietPlan.Description
                })
                .FirstOrDefaultAsync();

            if (userDietPlan == null)
            {
                return new List<object>();
            }

            return new List<object> { userDietPlan };
        }

        public async Task<List<object>> GetSuggestedMealsAsync(int limit = 10)
        {
            try
            {
                // Simple: Get active meals with join, no complex calculations
                var suggestedMeals = await (from m in _context.Meals
                                          where m.StatusId == 1
                                          join c in _context.MealCategories on m.CategoryId equals c.Id into categoryGroup
                                          from c in categoryGroup.DefaultIfEmpty()
                                          join s in _context.MealStatuses on m.StatusId equals s.Id into statusGroup
                                          from s in statusGroup.DefaultIfEmpty()
                                          orderby m.Mealid descending
                                          select new
                                          {
                                              mealid = m.Mealid,
                                              name = m.Name ?? string.Empty,
                                              calories = m.Calories ?? 0,
                                              protein = m.Protein ?? 0,
                                              carbs = m.Carbs ?? 0,
                                              fat = m.Fat ?? 0,
                                              cookingTime = m.Cookingtime ?? 0,
                                              imageUrl = m.ImageUrl ?? string.Empty,
                                              isPremium = m.IsPremium ?? false,
                                              diettype = m.Diettype ?? string.Empty,
                                              categoryName = c != null ? c.Name : "Món ăn",
                                              statusName = s != null ? s.Name : "Published",
                                              price = m.Price ?? 0,
                                              description = m.Description ?? string.Empty
                                          })
                                          .Take(limit)
                                          .ToListAsync();

                return suggestedMeals.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetSuggestedMealsAsync ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<List<object>> GetPopularMealsAsync(int limit = 10)
        {
            try
            {
                // Simple: Get active meals with join, no complex calculations
                var popularMeals = await (from m in _context.Meals
                                         where m.StatusId == 1
                                         join c in _context.MealCategories on m.CategoryId equals c.Id into categoryGroup
                                         from c in categoryGroup.DefaultIfEmpty()
                                         join s in _context.MealStatuses on m.StatusId equals s.Id into statusGroup
                                         from s in statusGroup.DefaultIfEmpty()
                                         orderby m.Mealid descending
                                         select new
                                         {
                                             mealid = m.Mealid,
                                             name = m.Name ?? string.Empty,
                                             calories = m.Calories ?? 0,
                                             protein = m.Protein ?? 0,
                                             carbs = m.Carbs ?? 0,
                                             fat = m.Fat ?? 0,
                                             cookingTime = m.Cookingtime ?? 0,
                                             imageUrl = m.ImageUrl ?? string.Empty,
                                             isPremium = m.IsPremium ?? false,
                                             diettype = m.Diettype ?? string.Empty,
                                             categoryName = c != null ? c.Name : "Món ăn",
                                             statusName = s != null ? s.Name : "Published",
                                             price = m.Price ?? 0,
                                             description = m.Description ?? string.Empty
                                         })
                                         .Take(limit)
                                         .ToListAsync();

                return popularMeals.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetPopularMealsAsync ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<(List<object> meals, int totalCount)> SearchMealsWithFiltersAsync(FilterSearchRequest request)
        {
            var query = _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Status)
                .Include(m => m.Mealingredients)
                    .ThenInclude(mi => mi.Ingredient)
                .Include(m => m.Mealplans)
                    .ThenInclude(mp => mp.Mealtime)
                .Where(m => m.StatusId == 1) // Active meals only
                .AsQueryable();

            // Apply diet type filter (either from request or user's diet plan for personal nutrition)
            if (!string.IsNullOrEmpty(request.DietType))
            {
                query = query.Where(m => m.Diettype == request.DietType);
            }
            else if (request.UserId.HasValue && request.UsePersonalNutrition == true)
            {
                var userDietPlan = await _context.Users
                    .Where(u => u.Userid == request.UserId.Value)
                    .Include(u => u.DietPlan)
                    .Select(u => u.DietPlan.Name)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(userDietPlan))
                {
                    query = query.Where(m => m.Diettype == userDietPlan);
                }
            }

            // Apply cooking time filter
            if (request.MaxCookingTime.HasValue)
            {
                query = query.Where(m => m.Cookingtime.HasValue && m.Cookingtime <= request.MaxCookingTime.Value);
            }

            // Apply calorie range filter
            if (request.MinCalories.HasValue)
            {
                query = query.Where(m => m.Calories >= request.MinCalories.Value);
            }
            if (request.MaxCalories.HasValue)
            {
                query = query.Where(m => m.Calories <= request.MaxCalories.Value);
            }

            // Apply ingredients filter - get meals that contain ANY of the selected ingredients
            if (request.Ingredients?.Any() == true)
            {
                // Get all meal IDs that contain any of the selected ingredients
                var allMealIds = new List<int>();
                
                foreach (var ingredient in request.Ingredients)
                {
                    var mealIds = await _context.Mealingredients
                        .Where(mi => mi.Ingredient.Name == ingredient && mi.Mealid.HasValue)
                        .Select(mi => mi.Mealid.Value)
                        .Distinct()
                        .ToListAsync();
                    
                    allMealIds.AddRange(mealIds);
                }
                
                // Remove duplicates
                allMealIds = allMealIds.Distinct().ToList();
                
                if (allMealIds.Any())
                {
                    query = query.Where(m => allMealIds.Contains(m.Mealid));
                }
                else
                {
                    // If no meals contain any selected ingredients, return empty result
                    query = query.Where(m => false);
                }
            }

            // Apply category and meal type filters
            if (request.Categories?.Any() == true || request.MealTypes?.Any() == true)
            {
                var categoryNames = new List<string>();
                
                // Add regular categories
                if (request.Categories?.Any() == true)
                {
                    categoryNames.AddRange(request.Categories);
                }
                
                // Add meal type categories
                if (request.MealTypes?.Any() == true)
                {
                    foreach (var mealType in request.MealTypes)
                    {
                        var categoryName = mealType switch
                        {
                            "Bữa sáng" => "Breakfast",
                            "Bữa trưa" => "Lunch", 
                            "Bữa tối" => "Dinner",
                            "Đồ ăn nhẹ" => "Snack",
                            _ => mealType
                        };
                        categoryNames.Add(categoryName);
                    }
                }
                
                // Remove duplicates
                categoryNames = categoryNames.Distinct().ToList();
                
                // Apply combined category filter
                query = query.Where(m => m.Category != null && categoryNames.Contains(m.Category.Name));
            }

            // Apply premium filter
            if (request.IsPremium.HasValue)
            {
                query = query.Where(m => m.IsPremium == request.IsPremium.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Get results with pagination
            var meals = await query
                .Select(m => new
                {
                    mealid = m.Mealid,
                    name = m.Name,
                    calories = m.Calories,
                    protein = m.Protein,
                    carbs = m.Carbs,
                    fat = m.Fat,
                    cookingTime = m.Cookingtime,
                    imageUrl = m.ImageUrl,
                    isPremium = m.IsPremium,
                    diettype = m.Diettype,
                    categoryName = m.Category != null ? m.Category.Name : null,
                    statusName = m.Status != null ? m.Status.Name : null,
                    price = m.Price,
                    description = m.Description
                })
                .Skip(request.Page * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return (meals.Cast<object>().ToList(), totalCount);
        }

        public async Task<List<object>> SearchMealsWithPersonalNutritionAsync(PersonalNutritionSearchRequest request, int userId)
        {
            // Get user's nutrition profile
            var userProfile = await _context.Users
                .Include(u => u.Healthprofiles)
                .FirstOrDefaultAsync(u => u.Userid == userId);

            if (userProfile?.Healthprofiles == null || !userProfile.Healthprofiles.Any())
            {
                throw new ArgumentException("User health profile not found");
            }

            var healthProfile = userProfile.Healthprofiles.First();
            
            // Build query based on user's nutrition goals
            var query = _context.Meals
                .Include(m => m.Category)
                .Include(m => m.Status)
                .Include(m => m.Mealingredients)
                    .ThenInclude(mi => mi.Ingredient)
                .Include(m => m.Mealplans)
                    .ThenInclude(mp => mp.Mealtime)
                .Where(m => m.StatusId == 1) // Active meals only
                .AsQueryable();

            // Apply calorie range based on user's goals
            if (healthProfile.Targetcalories.HasValue)
            {
                var targetCalories = healthProfile.Targetcalories.Value;
                var calorieRange = targetCalories * 0.2; // ±20% of target
                
                query = query.Where(m => m.Calories >= targetCalories - calorieRange && 
                                       m.Calories <= targetCalories + calorieRange);
            }

            // Apply additional filters from request
            if (!string.IsNullOrEmpty(request.DietType))
            {
                query = query.Where(m => m.Diettype == request.DietType);
            }

            if (request.MaxCookingTime.HasValue)
            {
                query = query.Where(m => m.Cookingtime.HasValue && m.Cookingtime <= request.MaxCookingTime.Value);
            }

            // Apply ingredients filter - get meals that contain ANY of the selected ingredients
            if (request.Ingredients?.Any() == true)
            {
                // Get all meal IDs that contain any of the selected ingredients
                var allMealIds = new List<int>();
                
                foreach (var ingredient in request.Ingredients)
                {
                    var mealIds = await _context.Mealingredients
                        .Where(mi => mi.Ingredient.Name == ingredient && mi.Mealid.HasValue)
                        .Select(mi => mi.Mealid.Value)
                        .Distinct()
                        .ToListAsync();
                    
                    allMealIds.AddRange(mealIds);
                }
                
                // Remove duplicates
                allMealIds = allMealIds.Distinct().ToList();
                
                if (allMealIds.Any())
                {
                    query = query.Where(m => allMealIds.Contains(m.Mealid));
                }
                else
                {
                    // If no meals contain any selected ingredients, return empty result
                    query = query.Where(m => false);
                }
            }

            // Get results
            var meals = await query
                .Select(m => new
                {
                    mealid = m.Mealid,
                    name = m.Name,
                    calories = m.Calories,
                    protein = m.Protein,
                    carbs = m.Carbs,
                    fat = m.Fat,
                    cookingTime = m.Cookingtime,
                    imageUrl = m.ImageUrl,
                    isPremium = m.IsPremium,
                    diettype = m.Diettype,
                    categoryName = m.Category != null ? m.Category.Name : null,
                    statusName = m.Status != null ? m.Status.Name : null,
                    price = m.Price,
                    description = m.Description
                })
                .Take(20) // Limit results
                .ToListAsync();

            return meals.Cast<object>().ToList();
        }
    }
}
