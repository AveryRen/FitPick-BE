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

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRegisterDto dto)
        {
            bool result = await _authService.RegisterAsync(dto);

            if (!result)
            {
                var error = new List<string> { "Email already exists or passwords do not match." };
                return BadRequest(ApiResponse<string>.ErrorResponse(error, "Register failed"));
            }

            return Ok(ApiResponse<string>.SuccessResponse(null, "Register successful"));
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
