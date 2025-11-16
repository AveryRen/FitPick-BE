using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitPick_EXE201.Data;
using FitPick_EXE201.Helpers;
using FitPick_EXE201.Services;
using FitPick_EXE201.Repositories.Interface;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilterController : ControllerBase
    {
        private readonly IFilterService _filterService;
        private readonly IPersonalizationService _personalizationService;
        private readonly FitPickContext _context;

        public FilterController(IFilterService filterService, IPersonalizationService personalizationService, FitPickContext context)
        {
            _filterService = filterService;
            _personalizationService = personalizationService;
            _context = context;
        }

        // Get all categories
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetCategories()
        {
            try
            {
                var categories = await _filterService.GetCategoriesAsync();

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    categories,
                    "Lấy danh sách danh mục thành công"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }

        // Get all ingredients with pagination
        [HttpGet("ingredients")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetIngredients([FromQuery] int page = 0, [FromQuery] int pageSize = 20)
        {
            try
            {
                var ingredients = await _filterService.GetIngredientsAsync(page, pageSize);

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    ingredients,
                    "Lấy danh sách nguyên liệu thành công"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }

        // Get all meal statuses
        [HttpGet("meal-statuses")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetMealStatuses()
        {
            try
            {
                var statuses = await _filterService.GetMealStatusesAsync();

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    statuses,
                    "Lấy danh sách trạng thái meal thành công"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }

        // Get all diet types from meals
        [HttpGet("diet-types")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetDietTypes()
        {
            try
            {
                var dietTypes = await _filterService.GetDietTypesAsync();

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    dietTypes,
                    "Lấy danh sách chế độ ăn thành công"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }

        // Get cooking time ranges
        [HttpGet("cooking-times")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetCookingTimes()
        {
            try
            {
                var cookingTimes = await _filterService.GetCookingTimesAsync();

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    cookingTimes,
                    "Lấy danh sách thời gian chế biến thành công"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }

        // Search meals with filters
        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<List<object>>>> SearchWithFilters([FromBody] FilterSearchRequest request)
        {
            try
            {
                // Get user ID from token if available
                var userId = GetUserIdFromToken();
                if (userId != null)
                {
                    request.UserId = userId.Value;
                }

                var (meals, totalCount) = await _filterService.SearchMealsWithFiltersAsync(request);

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    meals,
                    $"Tìm thấy {totalCount} món ăn phù hợp"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }
        // Search meals with personal nutrition applied
        [HttpPost("search-with-nutrition")]
        public async Task<ActionResult<ApiResponse<List<object>>>> SearchWithPersonalNutrition([FromBody] PersonalNutritionSearchRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(ApiResponse<List<object>>.ErrorResponse(
                        new List<string> { "UserId not found in token" }, "Unauthorized"));

                var meals = await _filterService.SearchMealsWithPersonalNutritionAsync(request, userId.Value);

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    meals,
                    "Tìm kiếm với dinh dưỡng cá nhân thành công"
                ));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message }, "Bad Request"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }

        // Get meal types (breakfast, lunch, dinner)
        [HttpGet("meal-types")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetMealTypes()
        {
            try
            {
                var mealTypes = await _filterService.GetMealTypesAsync();

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    mealTypes,
                    "Lấy danh sách loại bữa ăn thành công"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }

        // Get suggested meals (personalized recommendations based on user profile)
        [HttpGet("suggested-meals")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetSuggestedMeals([FromQuery] int limit = 10)
        {
            try
            {
                Console.WriteLine($"🔍 GetSuggestedMeals Controller: Starting with limit={limit}");
                var userId = GetUserIdFromToken();
                Console.WriteLine($"🔍 GetSuggestedMeals Controller: userId={userId?.ToString() ?? "null"}");
                
                // If user is authenticated, use personalized recommendations
                if (userId.HasValue)
                {
                    try
                    {
                        Console.WriteLine($"🔍 GetSuggestedMeals Controller: Using personalized recommendations for user {userId.Value}");
                        var recommendations = await _personalizationService.GenerateRecommendationsAsync(userId.Value, limit);
                        var recommendationsList = recommendations.ToList();
                        Console.WriteLine($"✅ GetSuggestedMeals Controller: Got {recommendationsList.Count} recommendations");
                        
                        // Get meal details for each recommendation
                        var suggestedMeals = new List<object>();
                        foreach (var rec in recommendationsList)
                        {
                            var meal = await _context.Meals
                                .Include(m => m.Category)
                                .Include(m => m.Status)
                                .FirstOrDefaultAsync(m => m.Mealid == rec.MealId);
                            
                            if (meal != null)
                            {
                                suggestedMeals.Add(new
                                {
                                    mealid = meal.Mealid,
                                    name = meal.Name ?? string.Empty,
                                    calories = meal.Calories ?? 0,
                                    protein = meal.Protein ?? 0,
                                    carbs = meal.Carbs ?? 0,
                                    fat = meal.Fat ?? 0,
                                    cookingTime = meal.Cookingtime ?? 0,
                                    imageUrl = meal.ImageUrl ?? string.Empty,
                                    isPremium = meal.IsPremium ?? false,
                                    diettype = meal.Diettype ?? string.Empty,
                                    categoryName = meal.Category?.Name ?? "Món ăn",
                                    statusName = meal.Status?.Name ?? "Published",
                                    price = meal.Price ?? 0,
                                    description = meal.Description ?? string.Empty,
                                    confidenceScore = rec.ConfidenceScore
                                });
                            }
                        }

                        Console.WriteLine($"✅ GetSuggestedMeals Controller: Returning {suggestedMeals.Count} personalized meals");
                        return Ok(ApiResponse<List<object>>.SuccessResponse(
                            suggestedMeals,
                            "Lấy danh sách món ăn gợi ý cá nhân hóa thành công"
                        ));
                    }
                    catch (Exception personalizationEx)
                    {
                        Console.WriteLine($"❌ GetSuggestedMeals Controller: Personalization failed: {personalizationEx.Message}");
                        Console.WriteLine($"❌ Stack trace: {personalizationEx.StackTrace}");
                        // Fallback to simple suggested meals
                        var suggestedMeals = await _filterService.GetSuggestedMealsAsync(limit);
                        return Ok(ApiResponse<List<object>>.SuccessResponse(
                            suggestedMeals,
                            "Lấy danh sách món ăn phổ biến thành công"
                        ));
                    }
                }
                else
                {
                    // Fallback to popular meals if user not authenticated
                    Console.WriteLine($"🔍 GetSuggestedMeals Controller: User not authenticated, using fallback");
                    var suggestedMeals = await _filterService.GetSuggestedMealsAsync(limit);
                    Console.WriteLine($"✅ GetSuggestedMeals Controller: Returning {suggestedMeals.Count} fallback meals");
                    return Ok(ApiResponse<List<object>>.SuccessResponse(
                        suggestedMeals,
                        "Lấy danh sách món ăn phổ biến thành công"
                    ));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetSuggestedMeals Controller ERROR: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }

        // Get popular meals (most frequently used in meal plans and meal histories)
        [HttpGet("popular-meals")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetPopularMeals([FromQuery] int limit = 10)
        {
            try
            {
                Console.WriteLine($"🔍 GetPopularMeals Controller: Starting with limit={limit}");
                var popularMeals = await _filterService.GetPopularMealsAsync(limit);
                Console.WriteLine($"✅ GetPopularMeals Controller: Returning {popularMeals.Count} meals");

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    popularMeals,
                    "Lấy danh sách món ăn phổ biến thành công"
                ));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetPopularMeals Controller ERROR: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi server"
                ));
            }
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
