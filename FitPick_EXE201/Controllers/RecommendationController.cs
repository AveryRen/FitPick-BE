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
                    new List<string> { "User không hợp lệ hoặc chưa đăng nhập." }, "Unauthorized"));

            var result = await _aiService.GetDrinkRecommendation(userId, timeOfDay, goal);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Gợi ý nước uống thành công"));
        }

        [HttpGet("mealplan")]
        [RequiresProUser("AI Meal Plan Generation")]
        public async Task<IActionResult> GenerateMealPlan([FromQuery] DateTime date, [FromQuery] string? healthGoal, [FromQuery] string? lifestyle)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "User không hợp lệ hoặc chưa đăng nhập." }, "Unauthorized"));

            var result = await _aiService.GenerateMealPlan(userId, date, healthGoal, lifestyle);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Tạo thực đơn thành công"));
        }

        [HttpGet("weeklymealplan")]
        [RequiresProUser("Weekly Meal Plan Generation")]
        public async Task<IActionResult> GenerateWeeklyMealPlan([FromQuery] string? healthGoal, [FromQuery] string? lifestyle)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "User không hợp lệ hoặc chưa đăng nhập." }, "Unauthorized"));

            var result = await _aiService.GenerateWeeklyMealPlanWithAI(userId, healthGoal, lifestyle);

            return Ok(ApiResponse<object>.SuccessResponse(result, "Tạo thực đơn 7 ngày thành công"));
        }

        // --- Helper private method ---
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}
