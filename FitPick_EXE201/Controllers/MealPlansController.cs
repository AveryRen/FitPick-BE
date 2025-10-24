using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace FitPick_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MealPlansController : ControllerBase
    {
        private readonly MealPlanService _mealPlanService;
        private readonly NotificationHelper _notificationHelper;
        private readonly UserLimitationService _limitationService;
        private readonly WeeklyMealPlanService _weeklyMealPlanService;

        public MealPlansController(
            MealPlanService mealPlanService, 
            NotificationHelper notificationHelper,
            UserLimitationService limitationService,
            WeeklyMealPlanService weeklyMealPlanService)
        {
            _mealPlanService = mealPlanService;
            _notificationHelper = notificationHelper;
            _limitationService = limitationService;
            _weeklyMealPlanService = weeklyMealPlanService;
        }

        // Lấy user id từ token (hợp nhất nhiều tên claim vì các chỗ khác có thể dùng "id" hoặc "UserId" hoặc NameIdentifier)
        private int? GetUserIdFromToken()
        {
            var claim = User.FindFirst("id")
                        ?? User.FindFirst("UserId")
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (claim == null || !int.TryParse(claim.Value, out int userId) || userId == 0)
                return null;
            return userId;
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
        public async Task<ActionResult<ApiResponse<Mealplan>>> GenerateMealPlan([FromQuery] DateTime date)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            // Kiểm tra giới hạn cho Free user
            var canCreate = await _limitationService.CanCreateMealPlanAsync(userId.Value);
            if (!canCreate)
            {
                var remaining = await _limitationService.GetRemainingMealPlansTodayAsync(userId.Value);
                return BadRequest(ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { $"Bạn đã đạt giới hạn tạo thực đơn trong ngày. Còn lại {remaining} lượt. Nâng cấp lên Premium để không giới hạn!" }, 
                    "Đã đạt giới hạn"));
            }

            try
            {
                var plan = await _mealPlanService.GenerateMealPlanAsync(userId.Value, DateOnly.FromDateTime(date));
                if (plan == null)
                    return BadRequest(ApiResponse<Mealplan>.ErrorResponse(
                        new List<string> { "Không thể tạo thực đơn. Vui lòng đảm bảo bạn đã cập nhật hồ sơ sức khỏe." }, 
                        "Thất bại"));

                // Tạo thông báo khi tạo meal plan thành công
                try
                {
                    await _notificationHelper.CreateMealPlanNotificationAsync(userId.Value, date);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating meal plan notification: {ex.Message}");
                }

                return Ok(ApiResponse<Mealplan>.SuccessResponse(plan, "Tạo thực đơn thành công"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GenerateMealPlan endpoint: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<Mealplan>.ErrorResponse(
                    new List<string> { "Đã xảy ra lỗi khi tạo thực đơn. Vui lòng thử lại sau." }, 
                    "Lỗi hệ thống"));
            }
        }

        [HttpPost("generate-weekly")]
        public async Task<ActionResult<ApiResponse<WeeklyMealPlanDto>>> GenerateWeeklyMealPlan([FromBody] WeeklyMealPlanRequest request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<WeeklyMealPlanDto>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            if (!DateTime.TryParse(request.WeekStartDate, out DateTime weekStart))
                return BadRequest(ApiResponse<WeeklyMealPlanDto>.ErrorResponse(
                    new List<string> { "Invalid week start date format" }, "Ngày bắt đầu tuần không hợp lệ"));

            try
            {
                var weeklyPlan = await _weeklyMealPlanService.GenerateWeeklyMealPlanAsync(userId.Value, weekStart);
                if (weeklyPlan == null)
                {
                    return BadRequest(ApiResponse<WeeklyMealPlanDto>.ErrorResponse(
                        new List<string> { "Không thể tạo thực đơn cả tuần" }, "Thất bại"));
                }

                return Ok(ApiResponse<WeeklyMealPlanDto>.SuccessResponse(weeklyPlan, "Đã sinh thực đơn cả tuần thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<WeeklyMealPlanDto>.ErrorResponse(
                    new List<string> { ex.Message }, "Chỉ Premium user mới có thể tạo thực đơn tuần"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WeeklyMealPlanDto>.ErrorResponse(
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

        [HttpGet("weekly")]
        public async Task<ActionResult<ApiResponse<WeeklyMealPlanDto>>> GetWeeklyMealPlan()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<WeeklyMealPlanDto>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            try
            {
                var weeklyPlan = await _weeklyMealPlanService.GetCurrentWeeklyMealPlanAsync(userId.Value);
                if (weeklyPlan == null)
                {
                    return NotFound(ApiResponse<WeeklyMealPlanDto>.ErrorResponse(
                        new List<string> { "Chưa có thực đơn tuần nào" }, "Không tìm thấy"));
                }

                return Ok(ApiResponse<WeeklyMealPlanDto>.SuccessResponse(weeklyPlan, "Lấy thực đơn tuần thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<WeeklyMealPlanDto>.ErrorResponse(
                    new List<string> { ex.Message }, "Chỉ Premium user mới có thể xem thực đơn tuần"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WeeklyMealPlanDto>.ErrorResponse(
                    new List<string> { ex.Message }, "Không thể lấy thực đơn tuần"));
            }
        }

        [HttpGet("limitation-info")]
        public async Task<ActionResult<ApiResponse<UserLimitationInfo>>> GetUserLimitationInfo()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<UserLimitationInfo>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            try
            {
                var limitationInfo = await _limitationService.GetUserLimitationInfoAsync(userId.Value);
                return Ok(ApiResponse<UserLimitationInfo>.SuccessResponse(limitationInfo, "Lấy thông tin giới hạn thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserLimitationInfo>.ErrorResponse(
                    new List<string> { ex.Message }, "Không thể lấy thông tin giới hạn"));
            }
        }
    }
}
