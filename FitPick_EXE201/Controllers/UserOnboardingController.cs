using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserOnboardingController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly HealthprofileService _healthprofileService;

        public UserOnboardingController(UserService userService, HealthprofileService healthprofileService)
        {
            _userService = userService;
            _healthprofileService = healthprofileService;
        }

        // POST api/user/profile - Lưu thông tin cá nhân
        [HttpPost("profile")]
        public async Task<ActionResult<ApiResponse<object>>> SaveUserProfile([FromBody] SaveUserProfileRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var result = await _userService.SaveUserProfileAsync(userId, request);
                
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Profile saved successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Failed to save profile" }, "Save profile failed"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Save profile failed"));
            }
        }

        // POST api/user/goals - Lưu mục tiêu
        [HttpPost("goals")]
        public async Task<ActionResult<ApiResponse<object>>> SaveUserGoals([FromBody] SaveUserGoalsRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var result = await _userService.SaveUserGoalsAsync(userId, request);
                
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Goals saved successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Failed to save goals" }, "Save goals failed"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Save goals failed"));
            }
        }

        // POST api/user/lifestyle - Lưu mức độ vận động
        [HttpPost("lifestyle")]
        public async Task<ActionResult<ApiResponse<object>>> SaveUserLifestyle([FromBody] SaveUserLifestyleRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var result = await _userService.SaveUserLifestyleAsync(userId, request);
                
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Lifestyle saved successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Failed to save lifestyle" }, "Save lifestyle failed"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Save lifestyle failed"));
            }
        }

        // POST api/user/diet-plan - Lưu chế độ ăn
        [HttpPost("diet-plan")]
        public async Task<ActionResult<ApiResponse<object>>> SaveUserDietPlan([FromBody] SaveUserDietPlanRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var result = await _userService.SaveUserDietPlanAsync(userId, request);
                
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Diet plan saved successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Failed to save diet plan" }, "Save diet plan failed"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Save diet plan failed"));
            }
        }

        // POST api/user/cooking-level - Lưu kỹ năng nấu ăn
        [HttpPost("cooking-level")]
        public async Task<ActionResult<ApiResponse<object>>> SaveUserCookingLevel([FromBody] SaveUserCookingLevelRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var result = await _userService.SaveUserCookingLevelAsync(userId, request);
                
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Cooking level saved successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Failed to save cooking level" }, "Save cooking level failed"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Save cooking level failed"));
            }
        }

        // POST api/user/complete-onboarding - Hoàn thành onboarding
        [HttpPost("complete-onboarding")]
        public async Task<ActionResult<ApiResponse<object>>> CompleteOnboarding()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var result = await _userService.CompleteOnboardingAsync(userId);
                
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Onboarding completed successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Failed to complete onboarding" }, "Complete onboarding failed"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Complete onboarding failed"));
            }
        }

        // GET api/user/profile - Lấy thông tin profile
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetUserProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var profile = await _userService.GetUserProfileAsync(userId);
                
                if (profile != null)
                {
                    return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile, "Profile retrieved successfully"));
                }
                else
                {
                    return NotFound(ApiResponse<UserProfileDto>.ErrorResponse(
                        new List<string> { "Profile not found" }, "Profile not found"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResponse(
                    new List<string> { ex.Message }, "Get profile failed"));
            }
        }
    }
}
