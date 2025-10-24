using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/pro-personalized")]
    [ApiController]
    [Authorize]
    public class ProPersonalizedController : ControllerBase
    {
        private readonly ProPersonalizedService _service;
        private readonly ReminderService _reminderService;

        public ProPersonalizedController(
            ProPersonalizedService service,
            ReminderService reminderService)
        {
            _service = service;
            _reminderService = reminderService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        /// <summary>
        /// Lấy gợi ý cá nhân hóa chuyên sâu dựa trên lịch sử ăn uống, sở thích, và mục tiêu
        /// Chỉ dành cho PRO users
        /// </summary>
        [HttpGet("deep-recommendations")]
        public async Task<ActionResult<ApiResponse<DeepRecommendationDto>>> GetDeepRecommendations()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(ApiResponse<DeepRecommendationDto>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            try
            {
                var recommendations = await _service.GetDeepPersonalizedRecommendationsAsync(userId);
                if (recommendations == null)
                {
                    return StatusCode(403, ApiResponse<DeepRecommendationDto>.ErrorResponse(
                        new List<string> { "Tính năng này chỉ dành cho người dùng Premium" }, 
                        "Premium Required"));
                }

                return Ok(ApiResponse<DeepRecommendationDto>.SuccessResponse(
                    recommendations, 
                    "Đã tạo gợi ý cá nhân hóa thành công"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting deep recommendations: {ex.Message}");
                return BadRequest(ApiResponse<DeepRecommendationDto>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Không thể tạo gợi ý"));
            }
        }

        /// <summary>
        /// Tạo nhắc nhở lịch trình ăn uống tự động cho PRO users
        /// </summary>
        [HttpPost("meal-reminders/auto-setup")]
        public async Task<ActionResult<ApiResponse<List<ReminderResponseDto>>>> SetupMealReminders()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(ApiResponse<List<ReminderResponseDto>>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            try
            {
                var reminders = await _service.SetupAutomaticMealRemindersAsync(userId);
                if (reminders == null)
                {
                    return StatusCode(403, ApiResponse<List<ReminderResponseDto>>.ErrorResponse(
                        new List<string> { "Tính năng này chỉ dành cho người dùng Premium" }, 
                        "Premium Required"));
                }

                return Ok(ApiResponse<List<ReminderResponseDto>>.SuccessResponse(
                    reminders, 
                    "Đã thiết lập nhắc nhở lịch trình ăn uống thành công"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error setting up meal reminders: {ex.Message}");
                return BadRequest(ApiResponse<List<ReminderResponseDto>>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Không thể thiết lập nhắc nhở"));
            }
        }

        /// <summary>
        /// Lấy phân tích dinh dưỡng chi tiết và xu hướng ăn uống
        /// </summary>
        [HttpGet("nutrition-insights")]
        public async Task<ActionResult<ApiResponse<NutritionInsightsDto>>> GetNutritionInsights(
            [FromQuery] int days = 7)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(ApiResponse<NutritionInsightsDto>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            try
            {
                var insights = await _service.GetNutritionInsightsAsync(userId, days);
                if (insights == null)
                {
                    return StatusCode(403, ApiResponse<NutritionInsightsDto>.ErrorResponse(
                        new List<string> { "Tính năng này chỉ dành cho người dùng Premium" }, 
                        "Premium Required"));
                }

                return Ok(ApiResponse<NutritionInsightsDto>.SuccessResponse(
                    insights, 
                    "Đã phân tích dinh dưỡng thành công"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting nutrition insights: {ex.Message}");
                return BadRequest(ApiResponse<NutritionInsightsDto>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Không thể phân tích dinh dưỡng"));
            }
        }

        /// <summary>
        /// Lấy các món ăn được đề xuất cá nhân hóa dựa trên thời gian trong ngày
        /// </summary>
        [HttpGet("time-based-suggestions")]
        public async Task<ActionResult<ApiResponse<TimeBasedSuggestionsDto>>> GetTimeBasedSuggestions()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(ApiResponse<TimeBasedSuggestionsDto>.ErrorResponse(
                    new List<string> { "UserId not found in token" }, "Unauthorized"));

            try
            {
                var suggestions = await _service.GetTimeBasedMealSuggestionsAsync(userId);
                if (suggestions == null)
                {
                    return StatusCode(403, ApiResponse<TimeBasedSuggestionsDto>.ErrorResponse(
                        new List<string> { "Tính năng này chỉ dành cho người dùng Premium" }, 
                        "Premium Required"));
                }

                return Ok(ApiResponse<TimeBasedSuggestionsDto>.SuccessResponse(
                    suggestions, 
                    "Đã tạo gợi ý món ăn theo thời gian"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting time-based suggestions: {ex.Message}");
                return BadRequest(ApiResponse<TimeBasedSuggestionsDto>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Không thể tạo gợi ý"));
            }
        }
    }
}
