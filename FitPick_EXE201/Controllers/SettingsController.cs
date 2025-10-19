using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Repositories.Interface;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/settings")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("privacy-policy")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> GetPrivacyPolicy()
        {
            try
            {
                var privacyPolicy = await _settingsService.GetPrivacyPolicyContentAsync();
                
                return Ok(ApiResponse<string>.SuccessResponse(privacyPolicy, "Privacy policy retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { ex.Message }, "Failed to retrieve privacy policy"));
            }
        }

        [HttpGet("terms-of-service")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> GetTermsOfService()
        {
            try
            {
                var termsOfService = await _settingsService.GetTermsOfServiceContentAsync();
                
                return Ok(ApiResponse<string>.SuccessResponse(termsOfService, "Terms of service retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { ex.Message }, "Failed to retrieve terms of service"));
            }
        }

        [HttpPut("update-profile")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                var result = await _settingsService.UpdateUserProfileAsync(userId, request);
                
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Failed to update profile" }, "Failed to update profile"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Profile updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Failed to update profile"));
            }
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                var result = await _settingsService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
                
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Current password is incorrect" }, "Current password is incorrect"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Failed to change password"));
            }
        }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
