using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/reminders")]
    [ApiController]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private readonly ReminderService _service;
        public ReminderController(ReminderService service)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // POST: api/reminders
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReminderResponseDto>>> Create([FromBody] ReminderCreateDto dto)
        {
            var created = await _service.CreateAsync(GetUserId(), dto);
            return Ok(ApiResponse<ReminderResponseDto>.SuccessResponse(
                created,
                "Reminder created successfully"
            ));
        }

        // GET: api/reminders/user
        [HttpGet("user")]
        public async Task<ActionResult<ApiResponse<List<ReminderResponseDto>>>> GetByUser()
        {
            var list = await _service.GetByUserIdAsync(GetUserId());
            return Ok(ApiResponse<List<ReminderResponseDto>>.SuccessResponse(
                list,
                "Reminders fetched successfully"
            ));
        }

        // PUT: api/reminders/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> Update(int id, [FromBody] ReminderCreateDto dto)
        {
            var ok = await _service.UpdateAsync(id, GetUserId(), dto);
            if (!ok)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Reminder not found" },
                    "Update failed"
                ));
            }

            return Ok(ApiResponse<string>.SuccessResponse(
                "Reminder updated successfully",
                "Reminder updated successfully"
            ));
        }

        // DELETE: api/reminders/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id, GetUserId());
            if (!ok)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Reminder not found" },
                    "Delete failed"
                ));
            }

            return Ok(ApiResponse<string>.SuccessResponse(
                "Reminder deleted successfully",
                "Reminder deleted successfully"
            ));
        }
    }
}
