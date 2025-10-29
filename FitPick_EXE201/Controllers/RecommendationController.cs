using FitPick_EXE201.Helpers;
using FitPick_EXE201.Middleware;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecommendationsController : ControllerBase
    {
        private readonly AiService _aiService;

        public RecommendationsController(AiService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("drinks")]
        public async Task<IActionResult> GetDrinkRecommendation([FromQuery] string? timeOfDay, [FromQuery] string? goal)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "User kh�ng h?p l? ho?c chua dang nh?p." }, "Unauthorized"));

            var result = await _aiService.GetDrinkRecommendation(userId, timeOfDay, goal);
            return Ok(ApiResponse<object>.SuccessResponse(result, "G?i � nu?c u?ng th�nh c�ng"));
        }

        [HttpGet("mealplan")]
        [RequiresProUser("AI Meal Plan Generation")]
        public async Task<IActionResult> GenerateMealPlan([FromQuery] DateTime date, [FromQuery] string? healthGoal, [FromQuery] string? lifestyle)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "User kh�ng h?p l? ho?c chua dang nh?p." }, "Unauthorized"));

            var result = await _aiService.GenerateMealPlan(userId, date, healthGoal, lifestyle);
            return Ok(ApiResponse<object>.SuccessResponse(result, "T?o th?c don th�nh c�ng"));
        }

        [HttpGet("drinknotification")]
        public async Task<IActionResult> GenerateDrinkNotification()
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "User kh�ng h?p l? ho?c chua dang nh?p." }, "Unauthorized"));

            var result = await _aiService.GenerateDrinkNotification(userId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "T?o th�ng b�o nh?c u?ng nu?c th�nh c�ng"));
        }

        [HttpGet("weeklymealplan")]
        [RequiresProUser("Weekly Meal Plan Generation")]
        public async Task<IActionResult> GenerateWeeklyMealPlan([FromQuery] string? healthGoal, [FromQuery] string? lifestyle)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "User kh�ng h?p l? ho?c chua dang nh?p." }, "Unauthorized"));

            // g?i d�ng t�n method trong AiService
            var result = await _aiService.GenerateWeeklyMealPlanWithAI(userId, healthGoal, lifestyle);

            return Ok(ApiResponse<object>.SuccessResponse(result, "T?o th?c don 7 ng�y th�nh c�ng"));
        }


        [HttpGet("meals")]
        public async Task<IActionResult> GetMealRecommendation([FromQuery] string? mealType, [FromQuery] string? goal)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "User kh�ng h?p l? ho?c chua dang nh?p." }, "Unauthorized"));

            var result = await _aiService.GetMealRecommendation(userId, mealType, goal);
            return Ok(ApiResponse<object>.SuccessResponse(result, "G?i � m�n an th�nh c�ng"));
        }


        // --- Helper private method ---
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}
