using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/notification-settings")]
    [ApiController]
    [Authorize]
    public class NotificationSettingsController : ControllerBase
    {
        private readonly FitPickContext _context;

        public NotificationSettingsController(FitPickContext context)
        {
            _context = context;
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId) || userId == 0)
                return null;
            return userId;
        }

        /// <summary>
        /// Lấy cài đặt thông báo của user hiện tại
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<UserNotificationSettingsDTO>>> GetNotificationSettings()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<UserNotificationSettingsDTO>
                        .ErrorResponse(new List<string> { "Invalid user token." }, "Unauthorized."));
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserNotificationSettingsDTO>
                        .ErrorResponse(new List<string> { "User not found." }, "User not found."));
                }

                var settings = new UserNotificationSettingsDTO
                {
                    UserId = user.Userid,
                    NotificationsEnabled = user.NotificationsEnabled ?? true
                };

                return Ok(ApiResponse<UserNotificationSettingsDTO>
                    .SuccessResponse(settings, "Notification settings retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserNotificationSettingsDTO>
                    .ErrorResponse(new List<string> { ex.Message }, "Internal server error."));
            }
        }

        /// <summary>
        /// Cập nhật cài đặt thông báo của user hiện tại
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<ApiResponse<UserNotificationSettingsDTO>>> UpdateNotificationSettings(
            [FromBody] UpdateNotificationSettingsRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<UserNotificationSettingsDTO>
                        .ErrorResponse(new List<string> { "Invalid user token." }, "Unauthorized."));
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserNotificationSettingsDTO>
                        .ErrorResponse(new List<string> { "User not found." }, "User not found."));
                }

                // Cập nhật notification settings
                user.NotificationsEnabled = request.NotificationsEnabled;
                user.Updatedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                await _context.SaveChangesAsync();

                var updatedSettings = new UserNotificationSettingsDTO
                {
                    UserId = user.Userid,
                    NotificationsEnabled = user.NotificationsEnabled ?? true
                };

                return Ok(ApiResponse<UserNotificationSettingsDTO>
                    .SuccessResponse(updatedSettings, "Notification settings updated successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserNotificationSettingsDTO>
                    .ErrorResponse(new List<string> { ex.Message }, "Internal server error."));
            }
        }
    }
}
