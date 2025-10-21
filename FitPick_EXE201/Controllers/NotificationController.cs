using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Helpers;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/notification")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly NotificationHelper _notificationHelper;
        private readonly FitPickContext _context;

        public NotificationController(
            NotificationService notificationService, 
            NotificationHelper notificationHelper,
            FitPickContext context)
        {
            _notificationService = notificationService;
            _notificationHelper = notificationHelper;
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
        /// Lấy danh sách thông báo của user hiện tại
        /// </summary>
        [HttpGet("user")]
        public async Task<ActionResult<ApiResponse<List<NotificationDTO>>>> GetUserNotifications([FromQuery] bool? onlyUnread = null)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<List<NotificationDTO>>
                        .ErrorResponse(new List<string> { "Invalid user token." }, "Unauthorized."));
                }

                var notifications = await _notificationService.GetNotificationsForUserAsync(userId.Value, onlyUnread);
                return Ok(ApiResponse<List<NotificationDTO>>
                    .SuccessResponse(notifications, "Notifications retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<NotificationDTO>>
                    .ErrorResponse(new List<string> { ex.Message }, "Internal server error."));
            }
        }

        /// <summary>
        /// Gửi thông báo (chủ yếu cho admin hoặc test)
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<NotificationDTO>>> SendNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<NotificationDTO>
                        .ErrorResponse(new List<string> { "Invalid user token." }, "Unauthorized."));
                }

                // Tạo thông báo cho user hiện tại
                await _notificationHelper.CreateSystemNotificationAsync(userId.Value, request.Title, request.Message);

                // Lấy thông báo vừa tạo để trả về
                var notifications = await _notificationService.GetNotificationsForUserAsync(userId.Value, false);
                var latestNotification = notifications.OrderByDescending(n => n.CreatedAt).FirstOrDefault();

                return Ok(ApiResponse<NotificationDTO>
                    .SuccessResponse(latestNotification, "Notification sent successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<NotificationDTO>
                    .ErrorResponse(new List<string> { ex.Message }, "Internal server error."));
            }
        }

        /// <summary>
        /// Đánh dấu thông báo là đã đọc
        /// </summary>
        [HttpPut("mark-read/{notificationId}")]
        public async Task<ActionResult<ApiResponse<NotificationDTO>>> MarkAsRead(int notificationId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<NotificationDTO>
                        .ErrorResponse(new List<string> { "Invalid user token." }, "Unauthorized."));
                }

                var result = await _notificationService.MarkAsReadAsync(notificationId, userId.Value);
                if (result == null)
                {
                    return NotFound(ApiResponse<NotificationDTO>
                        .ErrorResponse(new List<string> { "Notification not found." }, "Notification not found."));
                }

                return Ok(ApiResponse<NotificationDTO>
                    .SuccessResponse(result, "Notification marked as read."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<NotificationDTO>
                    .ErrorResponse(new List<string> { ex.Message }, "Internal server error."));
            }
        }

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        [HttpDelete("delete/{notificationId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(int notificationId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<bool>
                        .ErrorResponse(new List<string> { "Invalid user token." }, "Unauthorized."));
                }

                var result = await _notificationService.DeleteNotificationAsync(notificationId, userId.Value);
                if (!result)
                {
                    return NotFound(ApiResponse<bool>
                        .ErrorResponse(new List<string> { "Notification not found." }, "Notification not found."));
                }

                return Ok(ApiResponse<bool>
                    .SuccessResponse(true, "Notification deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>
                    .ErrorResponse(new List<string> { ex.Message }, "Internal server error."));
            }
        }

        /// <summary>
        /// Lấy tất cả loại thông báo
        /// </summary>
        [HttpGet("types")]
        public async Task<ActionResult<ApiResponse<List<NotificationTypeDTO>>>> GetNotificationTypes()
        {
            try
            {
                var types = await _notificationService.GetAllNotificationTypesAsync();
                return Ok(ApiResponse<List<NotificationTypeDTO>>
                    .SuccessResponse(types, "Notification types retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<NotificationTypeDTO>>
                    .ErrorResponse(new List<string> { ex.Message }, "Internal server error."));
            }
        }
    }

    // Request DTOs
    public class SendNotificationRequest
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int? TypeId { get; set; }
        public string? ScheduleAt { get; set; }
    }
}