using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/userProfile")]
    [ApiController]
    [Authorize(Roles = "Admin,Premium,User")]

    public class UserProfileController : ControllerBase
    {
        private readonly UserService _userService;

        public UserProfileController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetUserById()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(ApiResponse<UserProfileDto>.ErrorResponse(
                    new List<string> { "User not found" }, "User not found"));
            }

            var dto = new UserProfileDto
            {
                Fullname = user.Fullname,
                Email = user.Email,
                GenderId = user.GenderId,
                Age = user.Age,
                Height = user.Height,
                Weight = user.Weight,
                Country = user.Country,
                City = user.City,
                AvatarUrl = user.AvatarUrl
            };

            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(dto, "User retrieved successfully"));
        }

        [HttpPut("updateProfile")]
        public async Task<ActionResult<ApiResponse<UpdateUserProfileDto>>> UpdateProfile(
             [FromBody] UpdateUserProfileRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Gọi service update mà không động tới avatar
            var updatedUser = await _userService.UpdateProfileAsync(userId, request);

            if (updatedUser == null)
            {
                return BadRequest(ApiResponse<UpdateUserProfileDto>.ErrorResponse(
                    new List<string> { "Update failed" }, "Could not update profile"));
            }

            return Ok(ApiResponse<UpdateUserProfileDto>.SuccessResponse(updatedUser, "Profile updated successfully"));
        }




        [HttpPut("changePassword")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] string newPassword)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _userService.ChangePasswordAsync(userId, newPassword);
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Change password failed" }, "Could not change password"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));
        }

        
        [HttpPut("changeAvatar")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<object>>> ChangeAvatar(
            [FromForm] ChangeAvatarRequest request,
            [FromServices] CloudinaryService cloudinary)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (request.Avatar == null || request.Avatar.Length == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Invalid file" }, "Avatar is required"));
            }

            string avatarUrl = await cloudinary.UploadFileAsync(request.Avatar);

            var result = await _userService.ChangeAvatarAsync(userId, avatarUrl);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Change avatar failed" }, "Could not change avatar"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, "Avatar updated successfully"));
        }

    }
}
