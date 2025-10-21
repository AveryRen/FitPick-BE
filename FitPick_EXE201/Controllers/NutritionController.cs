using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class NutritionController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IPersonalizationService _personalizationService;

        public NutritionController(UserService userService, IPersonalizationService personalizationService)
        {
            _userService = userService;
            _personalizationService = personalizationService;
        }

        // GET api/user/nutrition-stats?date=YYYY-MM-DD
        [HttpGet("nutrition-stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetNutritionStats([FromQuery] string? date = null)
        {
            try
            {
                // Try multiple claim types to get user ID
                var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId) || userId == 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(new List<string> { "User not authenticated" }, "Unauthorized"));
                }

                // Parse date or use today
                var targetDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

                // Get user profile to calculate nutrition goals
                var userProfile = await _userService.GetUserProfileAsync(userId);
                if (userProfile == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { "User profile not found" }, "Profile Not Found"));
                }

                // Use target calories from profile or calculate if not available
                var targetCalories = userProfile.TargetCalories ?? CalculateNutritionGoals(userProfile).TargetCalories;

                // Calculate consumed calories from actual meal history
                var consumedCalories = await _userService.GetConsumedCaloriesAsync(userId, targetDate);

                // For demo purposes, return mock data with calculated goals
                // In real app, you would query actual consumed meals for the date
                var nutritionStats = new
                {
                    targetCalories = targetCalories,
                    consumedCalories = consumedCalories, // Calculated from sample meals
                    starch = new { current = (int)(consumedCalories * 0.50 / 4), target = (int)(targetCalories * 0.50 / 4) }, // 50% carbs
                    protein = new { current = (int)(consumedCalories * 0.25 / 4), target = (int)(targetCalories * 0.25 / 4) }, // 25% protein
                    fat = new { current = (int)(consumedCalories * 0.25 / 9), target = (int)(targetCalories * 0.25 / 9) } // 25% fat
                };

                return Ok(ApiResponse<object>.SuccessResponse(nutritionStats, "Nutrition stats retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, "Failed to get nutrition stats"));
            }
        }

        // GET api/user/detailed-nutrition-stats?date=YYYY-MM-DD
        [HttpGet("detailed-nutrition-stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetDetailedNutritionStats([FromQuery] string? date = null)
        {
            try
            {
                // Try multiple claim types to get user ID
                var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId) || userId == 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(new List<string> { "User not authenticated" }, "Unauthorized"));
                }

                // Parse date or use today
                var targetDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

                // Get user profile to calculate nutrition goals
                var userProfile = await _userService.GetUserProfileAsync(userId);
                if (userProfile == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { "User profile not found" }, "Profile Not Found"));
                }

                // Calculate detailed nutrition targets based on user profile
                var targetCalories = userProfile.TargetCalories ?? CalculateNutritionGoals(userProfile).TargetCalories;
                
                // Calculate consumed calories from actual meal history
                var consumedCalories = await _userService.GetConsumedCaloriesAsync(userId, targetDate);

                // Calculate detailed nutrition stats with realistic targets
                var detailedNutritionStats = new
                {
                    sugar = new { current = (int)(consumedCalories * 0.08 / 4), target = Math.Max(25, (int)(targetCalories * 0.10 / 4)), unit = "g" },
                    sodium = new { current = (int)(consumedCalories * 0.15), target = 2300, unit = "mg" },
                    saturatedFat = new { current = (int)(consumedCalories * 0.08 / 9), target = Math.Max(15, (int)(targetCalories * 0.10 / 9)), unit = "g" },
                    calcium = new { current = (int)(consumedCalories * 0.8), target = userProfile.Gender?.ToLower().Contains("nam") == true ? 1000 : 1200, unit = "mg" },
                    vitaminD = new { current = (int)(consumedCalories * 0.3), target = 600, unit = "IU" }
                };

                return Ok(ApiResponse<object>.SuccessResponse(detailedNutritionStats, "Detailed nutrition stats retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, "Failed to get detailed nutrition stats"));
            }
        }

        // GET api/user/meals?date=YYYY-MM-DD
        [HttpGet("meals")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserMeals([FromQuery] string? date = null)
        {
            try
            {
                // Try multiple claim types to get user ID
                var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId) || userId == 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(new List<string> { "User not authenticated" }, "Unauthorized"));
                }

                // Parse date or use today
                var targetDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

                // For demo purposes, return sample meals
                // In real app, you would query actual user meals for the date
                var meals = new List<object>
                {
                    new { id = "1", title = "Cơm gà nướng", calories = "450 kcal", time = "25 phút", image = new { uri = "https://monngonmoingay.com/wp-content/uploads/2021/04/salad-bi-do-500.jpg" }, tag = "Đã dùng", isLocked = false },
                    new { id = "2", title = "Salad bơ cá hồi", calories = "350 kcal", time = "15 phút", image = new { uri = "https://monngonmoingay.com/wp-content/uploads/2021/04/salad-bi-do-500.jpg" }, tag = "Đã dùng", isLocked = false },
                    new { id = "3", title = "Súp gà nấm", calories = "280 kcal", time = "20 phút", image = new { uri = "https://monngonmoingay.com/wp-content/uploads/2021/04/salad-bi-do-500.jpg" }, tag = "Đã dùng", isLocked = false }
                };

                return Ok(ApiResponse<object>.SuccessResponse(meals, "User meals retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, "Failed to get user meals"));
            }
        }

        // =====================================================
        // PERSONALIZATION ENDPOINTS
        // =====================================================

        // GET api/user/preferences
        [HttpGet("preferences")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserMealPreferenceDto>>>> GetUserPreferences()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var preferences = await _personalizationService.GetUserPreferencesAsync(userId);
                return Ok(ApiResponse<IEnumerable<UserMealPreferenceDto>>.SuccessResponse(preferences, "User preferences retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<UserMealPreferenceDto>>.ErrorResponse(new List<string> { ex.Message }, "Error retrieving preferences"));
            }
        }

        // POST api/user/preferences
        [HttpPost("preferences")]
        public async Task<ActionResult<ApiResponse<UserMealPreferenceDto>>> CreatePreference([FromBody] CreateUserMealPreferenceDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var preference = await _personalizationService.CreatePreferenceAsync(userId, dto);
                return Ok(ApiResponse<UserMealPreferenceDto>.SuccessResponse(preference, "Preference created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserMealPreferenceDto>.ErrorResponse(new List<string> { ex.Message }, "Error creating preference"));
            }
        }

        // GET api/user/ratings
        [HttpGet("ratings")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserMealRatingDto>>>> GetUserRatings()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var ratings = await _personalizationService.GetUserRatingsAsync(userId);
                return Ok(ApiResponse<IEnumerable<UserMealRatingDto>>.SuccessResponse(ratings, "User ratings retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<UserMealRatingDto>>.ErrorResponse(new List<string> { ex.Message }, "Error retrieving ratings"));
            }
        }

        // POST api/user/ratings
        [HttpPost("ratings")]
        public async Task<ActionResult<ApiResponse<UserMealRatingDto>>> CreateRating([FromBody] CreateUserMealRatingDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var rating = await _personalizationService.CreateRatingAsync(userId, dto);
                return Ok(ApiResponse<UserMealRatingDto>.SuccessResponse(rating, "Rating created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserMealRatingDto>.ErrorResponse(new List<string> { ex.Message }, "Error creating rating"));
            }
        }

        // GET api/user/nutrition-goals
        [HttpGet("nutrition-goals")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserNutritionGoalDto>>>> GetUserNutritionGoals()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var goals = await _personalizationService.GetUserGoalsAsync(userId);
                return Ok(ApiResponse<IEnumerable<UserNutritionGoalDto>>.SuccessResponse(goals, "Nutrition goals retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<UserNutritionGoalDto>>.ErrorResponse(new List<string> { ex.Message }, "Error retrieving nutrition goals"));
            }
        }

        // POST api/user/nutrition-goals
        [HttpPost("nutrition-goals")]
        public async Task<ActionResult<ApiResponse<UserNutritionGoalDto>>> CreateNutritionGoal([FromBody] CreateUserNutritionGoalDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var goal = await _personalizationService.CreateGoalAsync(userId, dto);
                return Ok(ApiResponse<UserNutritionGoalDto>.SuccessResponse(goal, "Nutrition goal created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserNutritionGoalDto>.ErrorResponse(new List<string> { ex.Message }, "Error creating nutrition goal"));
            }
        }

        // GET api/user/nutrition-history?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD
        [HttpGet("nutrition-history")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserNutritionHistoryDto>>>> GetUserNutritionHistory([FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                DateTime? start = string.IsNullOrEmpty(startDate) ? null : DateTime.Parse(startDate);
                DateTime? end = string.IsNullOrEmpty(endDate) ? null : DateTime.Parse(endDate);
                
                var history = await _personalizationService.GetUserHistoryAsync(userId, start, end);
                return Ok(ApiResponse<IEnumerable<UserNutritionHistoryDto>>.SuccessResponse(history, "Nutrition history retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<UserNutritionHistoryDto>>.ErrorResponse(new List<string> { ex.Message }, "Error retrieving nutrition history"));
            }
        }

        // GET api/user/recommendations
        [HttpGet("recommendations")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserMealRecommendationDto>>>> GetUserRecommendations([FromQuery] bool? isViewed = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var recommendations = await _personalizationService.GetUserRecommendationsAsync(userId, isViewed);
                return Ok(ApiResponse<IEnumerable<UserMealRecommendationDto>>.SuccessResponse(recommendations, "Recommendations retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<UserMealRecommendationDto>>.ErrorResponse(new List<string> { ex.Message }, "Error retrieving recommendations"));
            }
        }

        // POST api/user/recommendations/generate
        [HttpPost("recommendations/generate")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserMealRecommendationDto>>>> GenerateRecommendations([FromQuery] int limit = 10)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var recommendations = await _personalizationService.GenerateRecommendationsAsync(userId, limit);
                return Ok(ApiResponse<IEnumerable<UserMealRecommendationDto>>.SuccessResponse(recommendations, "Recommendations generated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<UserMealRecommendationDto>>.ErrorResponse(new List<string> { ex.Message }, "Error generating recommendations"));
            }
        }

        // GET api/user/exclusions
        [HttpGet("exclusions")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserMealExclusionDto>>>> GetUserExclusions()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var exclusions = await _personalizationService.GetUserExclusionsAsync(userId);
                return Ok(ApiResponse<IEnumerable<UserMealExclusionDto>>.SuccessResponse(exclusions, "Exclusions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<UserMealExclusionDto>>.ErrorResponse(new List<string> { ex.Message }, "Error retrieving exclusions"));
            }
        }

        // POST api/user/exclusions
        [HttpPost("exclusions")]
        public async Task<ActionResult<ApiResponse<UserMealExclusionDto>>> CreateExclusion([FromBody] CreateMealExclusionDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var exclusion = await _personalizationService.CreateExclusionAsync(userId, dto.MealId, dto.ExclusionType, dto.Reason, dto.IsPermanent, dto.ExcludedUntil);
                return Ok(ApiResponse<UserMealExclusionDto>.SuccessResponse(exclusion, "Exclusion created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserMealExclusionDto>.ErrorResponse(new List<string> { ex.Message }, "Error creating exclusion"));
            }
        }

        // GET api/user/patterns
        [HttpGet("patterns")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserMealPatternDto>>>> GetUserPatterns()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var patterns = await _personalizationService.GetUserPatternsAsync(userId);
                return Ok(ApiResponse<IEnumerable<UserMealPatternDto>>.SuccessResponse(patterns, "Patterns retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<UserMealPatternDto>>.ErrorResponse(new List<string> { ex.Message }, "Error retrieving patterns"));
            }
        }

        // Helper method to calculate nutrition goals
        private (int TargetCalories, int TargetProtein, int TargetCarbs, int TargetFat) CalculateNutritionGoals(UserProfileDto userProfile)
        {
            // Basic calculation based on user profile
            // This is a simplified version - in real app, you'd use more sophisticated formulas
            
            var baseCalories = 2000; // Base calories for adult
            var targetCalories = baseCalories;
            
            // Adjust based on gender
            if (userProfile.Gender?.ToLower().Contains("nam") == true)
            {
                targetCalories += 200; // Men typically need more calories
            }
            
            // Adjust based on activity level (simplified)
            if (userProfile.ActivityLevel?.ToLower().Contains("active") == true)
            {
                targetCalories += 300;
            }
            else if (userProfile.ActivityLevel?.ToLower().Contains("sedentary") == true)
            {
                targetCalories -= 200;
            }
            
            // Calculate macronutrients (simplified ratios)
            var targetProtein = (int)(targetCalories * 0.25 / 4); // 25% protein
            var targetCarbs = (int)(targetCalories * 0.50 / 4);   // 50% carbs
            var targetFat = (int)(targetCalories * 0.25 / 9);     // 25% fat
            
            return (targetCalories, targetProtein, targetCarbs, targetFat);
        }
    }
}
