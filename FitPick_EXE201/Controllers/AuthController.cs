using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitPick_EXE201.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly EmailVerificationService _emailService;


        public AuthController(AuthService authService, EmailVerificationService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRegisterDto dto)
        {
            // Thử đăng ký
            var registerResult = await _authService.RegisterAsync(dto);

            if (!registerResult)
            {
                var error = new List<string> { "Email already exists or passwords do not match." };
                return BadRequest(ApiResponse<string>.ErrorResponse(error, "Register failed"));
            }

            // Gửi email xác thực
            var verifySent = await _emailService.RequestEmailVerificationAsync(dto.Email);
            var message = verifySent
                ? "Register successful. Verification code sent to your email."
                : "Register successful, but failed to send verification code.";

            return Ok(ApiResponse<string>.SuccessResponse(null, message));
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountLoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null || result.User == null) 
            {
                return Unauthorized(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Invalid email/password or inactive account." }, "Login failed"));
            }

            var account = result.User;

            var response = new
            {
                token = result.AccessToken,
                refreshToken = result.RefreshToken,
                expiresIn = result.ExpiresIn,
                user = new
                {
                    account.Userid,
                    account.Fullname,
                    account.Email,
                    RoleId = account.Role?.Id ?? account.RoleId,
                    Role = account.Role?.Name ?? "(unknown)"
                }
            }; 

            return Ok(ApiResponse<object>.SuccessResponse(response, "Login successful"));
        }



        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            var result = _authService.RefreshAccessToken(refreshToken);
            if (result == null)
                return Unauthorized(ApiResponse<string>.ErrorResponse(
                    new[] { "Invalid or expired refresh token" }.ToList(),
                    "Refresh token failed"
                ));

            return Ok(ApiResponse<AuthResultDto>.SuccessResponse(result, "Token refreshed successfully"));
        }
    }
}
