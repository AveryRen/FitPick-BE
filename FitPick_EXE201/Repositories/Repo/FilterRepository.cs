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
            // Get popular meals (most frequently used in meal plans)
            var suggestedMeals = await _context.Meals
                .Where(m => m.StatusId == 1) // Active meals only
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
                    categoryName = m.Category.Name,
                    statusName = m.Status.Name,
                    price = m.Price,
                    description = m.Description,
                    popularityScore = m.Mealplans.Count() // Count how many times this meal is used
                })
                .OrderByDescending(m => m.popularityScore)
                .ThenByDescending(m => m.mealid)
                .Take(limit)
                .ToListAsync();

            return suggestedMeals.Cast<object>().ToList();
        }

        public async Task<(List<object> meals, int totalCount)> SearchMealsWithFiltersAsync(FilterSearchRequest request)
        {
            // Debug: Check total active meals
            var totalActiveMeals = await _context.Meals.CountAsync(m => m.StatusId == 1);
            Console.WriteLine($"Debug: Total active meals in database: {totalActiveMeals}");
            
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
                Console.WriteLine($"Debug: Applying diet type filter from request: '{request.DietType}'");
                query = query.Where(m => m.Diettype == request.DietType);
                
                // Debug: Check how many meals match the diet type filter
                var dietTypeCount = await query.CountAsync();
                Console.WriteLine($"Debug: Found {dietTypeCount} meals matching diet type '{request.DietType}'");
            }
            else if (request.UserId.HasValue && request.UsePersonalNutrition == true)
            {
                Console.WriteLine($"Debug: Personal nutrition enabled, applying user's diet plan");
                Console.WriteLine($"Debug: User ID provided: {request.UserId.Value}");
                var userDietPlan = await _context.Users
                    .Where(u => u.Userid == request.UserId.Value)
                    .Include(u => u.DietPlan)
                    .Select(u => u.DietPlan.Name)
                    .FirstOrDefaultAsync();

                Console.WriteLine($"Debug: User diet plan: '{userDietPlan}'");
                if (!string.IsNullOrEmpty(userDietPlan))
                {
                    query = query.Where(m => m.Diettype == userDietPlan);
                    
                    // Debug: Check how many meals match the user's diet plan
                    var dietPlanCount = await query.CountAsync();
                    Console.WriteLine($"Debug: Found {dietPlanCount} meals matching user's diet plan '{userDietPlan}'");
                }
                else
                {
                    Console.WriteLine("Debug: User has no diet plan assigned");
                }
            }
            else
            {
                Console.WriteLine("Debug: No diet type filter applied (normal filter mode)");
            }

            // Apply cooking time filter
            if (request.MaxCookingTime.HasValue)
            {
                Console.WriteLine($"Debug: Applying cooking time filter <= {request.MaxCookingTime.Value} minutes");
                query = query.Where(m => m.Cookingtime.HasValue && m.Cookingtime <= request.MaxCookingTime.Value);
            }

            // Apply calorie range filter
            if (request.MinCalories.HasValue)
            {
                Console.WriteLine($"Debug: Applying min calories filter: >= {request.MinCalories.Value}");
                query = query.Where(m => m.Calories >= request.MinCalories.Value);
                var minCalCount = await query.CountAsync();
                Console.WriteLine($"Debug: Found {minCalCount} meals with calories >= {request.MinCalories.Value}");
            }
            if (request.MaxCalories.HasValue)
            {
                Console.WriteLine($"Debug: Applying max calories filter: <= {request.MaxCalories.Value}");
                query = query.Where(m => m.Calories <= request.MaxCalories.Value);
                var maxCalCount = await query.CountAsync();
                Console.WriteLine($"Debug: Found {maxCalCount} meals with calories <= {request.MaxCalories.Value}");
            }

            // Apply ingredients filter - get meals that contain ANY of the selected ingredients
            if (request.Ingredients?.Any() == true)
            {
                Console.WriteLine($"Debug: Applying ingredients filter: {string.Join(", ", request.Ingredients)}");
                // Get all meal IDs that contain any of the selected ingredients
                var allMealIds = new List<int>();
                
                foreach (var ingredient in request.Ingredients)
                {
                    var mealIds = await _context.Mealingredients
                        .Where(mi => mi.Ingredient.Name == ingredient && mi.Mealid.HasValue)
                        .Select(mi => mi.Mealid.Value)
                        .Distinct()
                        .ToListAsync();
                    
                    Console.WriteLine($"Debug: Found {mealIds.Count} meals with ingredient '{ingredient}'");
                    allMealIds.AddRange(mealIds);
                }
                
                // Remove duplicates
                allMealIds = allMealIds.Distinct().ToList();
                Console.WriteLine($"Debug: Total unique meals with any selected ingredients: {allMealIds.Count}");
                
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
                    Console.WriteLine($"Debug: Adding regular categories: {string.Join(", ", request.Categories)}");
                }
                
                // Add meal type categories
                if (request.MealTypes?.Any() == true)
                {
                    Console.WriteLine($"Debug: Applying meal types filter: {string.Join(", ", request.MealTypes)}");
                    
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
                    
                    Console.WriteLine($"Debug: Adding meal type categories: {string.Join(", ", request.MealTypes)} -> {string.Join(", ", request.MealTypes.Select(mt => mt switch { "Bữa sáng" => "Breakfast", "Bữa trưa" => "Lunch", "Bữa tối" => "Dinner", "Đồ ăn nhẹ" => "Snack", _ => mt }))}");
                }
                
                // Remove duplicates
                categoryNames = categoryNames.Distinct().ToList();
                Console.WriteLine($"Debug: Final category filter: {string.Join(", ", categoryNames)}");
                
                // Apply combined category filter
                query = query.Where(m => m.Category != null && categoryNames.Contains(m.Category.Name));
                
                // Debug: Check how many meals match the category filter
                var matchingMealsCount = await query.CountAsync();
                Console.WriteLine($"Debug: Found {matchingMealsCount} meals matching categories");
            }

            // Apply premium filter
            if (request.IsPremium.HasValue)
            {
                Console.WriteLine($"Debug: Applying premium filter: {request.IsPremium.Value}");
                query = query.Where(m => m.IsPremium == request.IsPremium.Value);
                var premiumCount = await query.CountAsync();
                Console.WriteLine($"Debug: Found {premiumCount} meals with premium = {request.IsPremium.Value}");
            }

            // Get total count
            var totalCount = await query.CountAsync();
            Console.WriteLine($"Debug: Final query count after all filters: {totalCount}");

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
