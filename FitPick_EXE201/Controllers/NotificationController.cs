using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Premium,Admin")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _service;

        public NotificationController(NotificationService service)
        {
            _service = service;
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }

        // POST api/notifications/send
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<NotificationDTO>>> SendNotification(string title, string message, int? typeId = null, DateTime? scheduleAt = null)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<NotificationDTO>.ErrorResponse(new() { "Invalid or missing user ID in token." }, "Invalid or missing user ID in token."));

            var result = await _service.SendNotificationAsync(userId.Value, title, message, typeId, scheduleAt);
            return Ok(ApiResponse<NotificationDTO>.SuccessResponse(result, "Notification sent successfully"));
        }

        // GET api/notifications/user?onlyUnread=true
        [HttpGet("user")]
        public async Task<ActionResult<ApiResponse<List<NotificationDTO>>>> GetUserNotifications([FromQuery] bool? onlyUnread)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<List<NotificationDTO>>.ErrorResponse(
                    new() { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."));

            var result = await _service.GetNotificationsForUserAsync(userId.Value, onlyUnread);
            return Ok(ApiResponse<List<NotificationDTO>>.SuccessResponse(result, "Get user notifications successfully"));
        }



        // PUT api/notifications/mark-read/5
        [HttpPut("mark-read/{id}")]
        public async Task<ActionResult<ApiResponse<NotificationDTO>>> MarkAsRead(int id)
        {
            try
            {
                var result = await _service.MarkAsReadAsync(id);
                return Ok(ApiResponse<NotificationDTO>.SuccessResponse(result, "Đánh dấu thông báo là đã đọc"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<NotificationDTO>.ErrorResponse(new() { ex.Message }, ex.Message));
            }
        }

        // DELETE api/notifications/delete/5
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(int id)
        {
            try
            {
                var result = await _service.DeleteNotificationAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Notification deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(new() { ex.Message }, ex.Message));
            }
        }

        // ================= Notification Type Management =================

        // POST api/notifications/types/create
        [HttpPost("types/create")]
        [Authorize(Roles = "Admin")] // chỉ Admin mới tạo loại thông báo
        public async Task<ActionResult<ApiResponse<NotificationTypeDTO>>> CreateType(string name)
        {
            try
            {
                var result = await _service.CreateTypeAsync(name);
                return Ok(ApiResponse<NotificationTypeDTO>.SuccessResponse(result, "Notification type created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<NotificationTypeDTO>.ErrorResponse(new() { ex.Message }, ex.Message));
            }
        }

        // GET api/notifications/types
        [HttpGet("types")]
        public async Task<ActionResult<ApiResponse<List<NotificationTypeDTO>>>> GetAllTypes()
        {
            var result = await _service.GetAllTypesAsync();
            return Ok(ApiResponse<List<NotificationTypeDTO>>.SuccessResponse(result, "Get all notification types successfully"));
        }

        // DELETE api/notifications/types/delete/5
        [HttpDelete("types/delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteType(int id)
        {
            try
            {
                var result = await _service.DeleteTypeAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Notification type deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(new() { ex.Message }, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(new() { ex.Message }, ex.Message));
            }
        }
    }
}
