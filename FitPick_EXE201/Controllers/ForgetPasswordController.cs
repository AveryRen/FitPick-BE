using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitPick_EXE201.Controllers
{
    [Route("api/forget_password")]
    [ApiController]
    public class ForgetPasswordController : ControllerBase
    {
        private readonly ForgetPasswordService _service;

        public ForgetPasswordController(ForgetPasswordService service)
        {
            _service = service;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestReset([FromBody] string email)
        {
            var success = await _service.RequestPasswordResetAsync(email);
            if (!success)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Email does not exist." }, "Failed to send reset code"));
            }

            return Ok(ApiResponse<string>.SuccessResponse(null, "Reset code has been sent to your email"));
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var success = await _service.ResetPasswordAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Invalid or expired verification code." }, "Password reset failed"));
            }

            return Ok(ApiResponse<string>.SuccessResponse(null, "Password has been reset successfully"));
        }
    }
}