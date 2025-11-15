using FitPick_EXE201.Helpers;
using FitPick_EXE201.Middleware;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MealPlansController : ControllerBase
    {
        private readonly MealPlanService _mealPlanService;
        private readonly NotificationHelper _notificationHelper;

        public MealPlansController(MealPlanService mealPlanService, NotificationHelper notificationHelper)
        {
            _mealPlanService = mealPlanService;
            _notificationHelper = notificationHelper;
        }

        [HttpGet("today")]
        public async Task<ActionResult<ApiResponse<List<TodayMealPlanDto>>>> GetTodayMealPlan()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<List<TodayMealPlanDto>>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            var today = DateTime.Now;
            var plans = await _mealPlanService.GetTodayMealPlanAsync(userId.Value, today);
            return Ok(ApiResponse<List<TodayMealPlanDto>>.SuccessResponse(plans, "L?y th?c don h�m nay th�nh c�ng"));
        }

        [HttpGet("date/{date}")]
        public async Task<ActionResult<ApiResponse<List<TodayMealPlanDto>>>> GetMealPlanByDate(string date)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<List<TodayMealPlanDto>>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            if (!DateTime.TryParse(date, out DateTime targetDate))
                return BadRequest(ApiResponse<List<TodayMealPlanDto>>.ErrorResponse(
                    new List<string> { "Invalid date format" }, "Ngày không hợp lệ"));

            var plans = await _mealPlanService.GetTodayMealPlanAsync(userId.Value, targetDate);
            return Ok(ApiResponse<List<TodayMealPlanDto>>.SuccessResponse(plans, $"Lấy thực đơn ngày {date} thành công"));
        }

        [HttpGet("user")]
        public async Task<ActionResult<ApiResponse<List<Mealplan>>>> GetUserMealPlans()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<List<Mealplan>>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            var plans = await _mealPlanService.GetUserMealPlansAsync(userId.Value);
            return Ok(ApiResponse<List<Mealplan>>.SuccessResponse(plans, "L?y to�n b? meal plan th�nh c�ng"));
        }

        [HttpPost("generate")]
        [RequiresProUser("Generate Meal Plan")]
        public async Task<ActionResult<ApiResponse<Mealplan>>> GenerateMealPlan([FromQuery] DateTime date)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(ApiResponse<Mealplan>.ErrorResponse(
                        new List<string> { "UserId not found in token" }, "Unauthorized"));

                var result = await _mealPlanService.GenerateMealPlanWithValidationAsync(userId.Value, DateOnly.FromDateTime(date));
                if (!result.Success)
                {
                    return BadRequest(ApiResponse<Mealplan>.ErrorResponse(
                        new List<string> { result.ErrorMessage }, result.ErrorCode));
                }

                var plan = result.Data;
                if (plan == null)
                {
                    return BadRequest(ApiResponse<Mealplan>.ErrorResponse(
                        new List<string> { "Không thể tạo meal plan. Vui lòng thử lại sau." }, "GENERATION_FAILED"));
                }

                // Tạo thông báo khi tạo meal plan thành công
                try
                {
                    await _notificationHelper.CreateMealPlanNotificationAsync(userId.Value, date);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating meal plan notification: {ex.Message}");
                }

                return Ok(ApiResponse<Mealplan>.SuccessResponse(plan, "Tạo meal plan thành công"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateMealPlan: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { $"Lỗi server: {ex.Message}" }, "Lỗi khi tạo meal plan"));
            }
        }

        [HttpPost("generate-weekly")]
        public async Task<ActionResult<ApiResponse<object>>> GenerateWeeklyMealPlan([FromBody] WeeklyMealPlanRequest request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            if (!DateTime.TryParse(request.WeekStartDate, out DateTime weekStart))
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Invalid week start date format" }, "Ngày bắt đầu tuần không hợp lệ"));

            try
            {
                // TODO: Implement weekly meal plan generation logic
                // For now, return a placeholder response
                var result = new
                {
                    message = "AI đang phân tích sở thích và tạo thực đơn cá nhân hóa cho cả tuần",
                    weekStartDate = request.WeekStartDate,
                    generatedPlans = new List<object>() // Placeholder
                };

                return Ok(ApiResponse<object>.SuccessResponse(result, "Đã sinh thực đơn cả tuần thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Không thể sinh thực đơn cả tuần"));
            }
        }

        [HttpPut("{id}/swap")]
        public async Task<ActionResult<ApiResponse<Mealplan>>> SwapMeal(int id, [FromQuery] int newMealId)
        {
            var plan = await _mealPlanService.SwapMealAsync(id, newMealId);
            if (plan == null)
                return NotFound(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "Meal plan kh�ng t?n t?i" }, "Th?t b?i"));

            return Ok(ApiResponse<Mealplan>.SuccessResponse(plan, "Ho�n d?i m�n th�nh c�ng"));
        }

        [HttpPut("replace-by-suggestion/{planId}")]
        public async Task<ActionResult<ApiResponse<Mealplan>>> ReplaceMealBySuggestion(int planId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            var plan = await _mealPlanService.ReplaceMealBySuggestionAsync(planId, userId.Value);
            if (plan == null)
                return NotFound(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "Meal plan không tồn tại" }, "Thất bại"));

            return Ok(ApiResponse<Mealplan>.SuccessResponse(plan, "Thay đổi món theo gợi ý thành công"));
        }

        [HttpPut("replace-by-favorites/{planId}")]
        public async Task<ActionResult<ApiResponse<Mealplan>>> ReplaceMealByFavorites(int planId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            var plan = await _mealPlanService.ReplaceMealByFavoritesAsync(planId, userId.Value);
            if (plan == null)
                return NotFound(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "Meal plan không tồn tại" }, "Thất bại"));

            return Ok(ApiResponse<Mealplan>.SuccessResponse(plan, "Thay đổi món theo danh sách yêu thích thành công"));
        }

        [HttpPost("add-meal")]
        public async Task<ActionResult<ApiResponse<Mealplan>>> AddMealToMenu([FromBody] AddMealToMenuRequest request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            var plan = await _mealPlanService.AddMealToMenuAsync(userId.Value, request.MealId, request.Date, request.MealTime);
            if (plan == null)
                return BadRequest(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "Không thể thêm món ăn vào thực đơn" }, "Thất bại"));

            return Ok(ApiResponse<Mealplan>.SuccessResponse(plan, "Đã thêm món ăn vào thực đơn thành công"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteMealPlan(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<bool>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            var success = await _mealPlanService.DeleteMealPlanAsync(id);
            if (success)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Xóa món ăn khỏi thực đơn thành công"));
            }
            else
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(
                    new List<string> { "Không tìm thấy món ăn trong thực đơn" }, "Not Found"));
            }
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;
            if (!int.TryParse(userIdClaim.Value, out int userId)) return null;
            return userId;
        }
    }
}
