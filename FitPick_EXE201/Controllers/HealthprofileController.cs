using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/healthprofiles")]
    [ApiController]
    [Authorize(Roles = "User,Premium,Admin")]
    public class HealthprofileController : ControllerBase
    {
        private readonly HealthprofileService _service;

        public HealthprofileController(HealthprofileService service)
        {
            _service = service;
        }

        // POST api/healthprofile
        [HttpPost]
        public async Task<ActionResult<ApiResponse<HealthprofileDTO>>> Create([FromBody] HealthprofileRequest request)
        {
            // Lấy UserId từ JWT
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<HealthprofileDTO>.ErrorResponse(
                    new List<string> { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));
            }

            var createdProfile = await _service.CreateHealthprofileAsync(userId, request);

            if (createdProfile == null)
            {
                return BadRequest(ApiResponse<HealthprofileDTO>.ErrorResponse(
                    new List<string> { "Failed to create health profile." },
                    "Failed to create health profile."
                ));
            }

            return Ok(ApiResponse<HealthprofileDTO>.SuccessResponse(
                createdProfile,
                "Health profile created successfully"
            ));
        }


        // GET api/healthprofile/user/{userid}
        [HttpGet("user")]
        public async Task<ActionResult<ApiResponse<HealthprofileDTO>>> GetByUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<HealthprofileDTO>.ErrorResponse(
                    new List<string> { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));
            }

            var profile = await _service.GetByUserIdAsync(userId);
            if (profile == null)
            {
                return NotFound(ApiResponse<HealthprofileDTO>.ErrorResponse(
                    new List<string> { "Health profile not found for this user." },
                    "Health profile not found for this user."
                ));
            }

            return Ok(ApiResponse<HealthprofileDTO>.SuccessResponse(profile, "Get By UserId successfully"));
        }
        [HttpGet("user/progress")]
        public async Task<ActionResult<ApiResponse<ProgressDto>>> GetUserProgress()
        {
            // Lấy userId từ JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<ProgressDto>.ErrorResponse(
                    new List<string> { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));
            }

            var progress = await _service.GetUserProgressAsync(userId);
            if (progress == null)
            {
                return NotFound(ApiResponse<ProgressDto>.ErrorResponse(
                    new List<string> { "No progress data found." },
                    "No progress data found."
                ));
            }

            return Ok(ApiResponse<ProgressDto>.SuccessResponse(progress, "User progress retrieved successfully"));
        }
        [HttpGet("user/goal")]
        public async Task<ActionResult<ApiResponse<UserGoalDto>>> GetUserGoal()
        {
            // Lấy userId từ JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<UserGoalDto>.ErrorResponse(
                    new List<string> { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));
            }

            // Gọi service
            var goal = await _service.GetUserGoalAsync(userId);
            if (goal == null)
            {
                return NotFound(ApiResponse<UserGoalDto>.ErrorResponse(
                    new List<string> { "No goal data found for this user." },
                    "No goal data found for this user."
                ));
            }

            return Ok(ApiResponse<UserGoalDto>.SuccessResponse(
                goal,
                "User goal retrieved successfully"
            ));
        }
    }
}