using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
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

        public MealPlansController(MealPlanService mealPlanService)
        {
            _mealPlanService = mealPlanService;
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
            return Ok(ApiResponse<List<TodayMealPlanDto>>.SuccessResponse(plans, "Lấy thực đơn hôm nay thành công"));
        }

        [HttpGet("user")]
        public async Task<ActionResult<ApiResponse<List<Mealplan>>>> GetUserMealPlans()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<List<Mealplan>>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            var plans = await _mealPlanService.GetUserMealPlansAsync(userId.Value);
            return Ok(ApiResponse<List<Mealplan>>.SuccessResponse(plans, "Lấy toàn bộ meal plan thành công"));
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ApiResponse<Mealplan>>> GenerateMealPlan([FromQuery] DateTime date)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            var plan = await _mealPlanService.GenerateMealPlanAsync(userId.Value, DateOnly.FromDateTime(date));
            if (plan == null)
                return BadRequest(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "Không thể tạo meal plan" }, "Thất bại"));

            return Ok(ApiResponse<Mealplan>.SuccessResponse(plan, "Tạo meal plan thành công"));
        }

        [HttpPut("{id}/swap")]
        public async Task<ActionResult<ApiResponse<Mealplan>>> SwapMeal(int id, [FromQuery] int newMealId)
        {
            var plan = await _mealPlanService.SwapMealAsync(id, newMealId);
            if (plan == null)
                return NotFound(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "Meal plan không tồn tại" }, "Thất bại"));

            return Ok(ApiResponse<Mealplan>.SuccessResponse(plan, "Hoán đổi món thành công"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMealPlan(int id)
        {
            var success = await _mealPlanService.DeleteMealPlanAsync(id);
            if (!success)
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Meal plan không tồn tại" }, "Thất bại"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Xóa meal plan thành công"));
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
