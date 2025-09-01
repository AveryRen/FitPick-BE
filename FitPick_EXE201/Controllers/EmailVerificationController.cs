using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitPick_EXE201.Controllers
{
    [Route("api/email_verification")]
    [ApiController]
    [AllowAnonymous]
    public class EmailVerificationController : ControllerBase
    {
        private readonly EmailVerificationService _service;

        public EmailVerificationController(EmailVerificationService service)
        {
            _service = service;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            var success = await _service.VerifyEmailAsync(dto.Email, dto.Code);

            if (!success)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Invalid or expired code." }, "Email verification failed"));
            }

            return Ok(ApiResponse<string>.SuccessResponse(null, "Email verified successfully"));
        }
    }
}