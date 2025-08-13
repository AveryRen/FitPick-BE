using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPick_EXE201.Controllers
{
    [Route("api/[controller]")]
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
        [HttpGet("user/{userid}")]
        public async Task<ActionResult<ApiResponse<HealthprofileDTO>>> GetByUserId(int userid)
        {
            var profile = await _service.GetByUserIdAsync(userid);
            if (profile == null)
                return NotFound(ApiResponse<HealthprofileDTO>.ErrorResponse(
                    new List<string> { "Health profile not found for this user." },
                    "Health profile not found for this user."
                ));

            return Ok(ApiResponse<HealthprofileDTO>.SuccessResponse(profile, "Get By UserId successfully"));
        }
    }
}