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
    [Route("api/users/me")]
    [ApiController]
    [Authorize(Roles = "Admin,Premium,User")]

    public class UserProfileController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ProUserService _proUserService;

        public UserProfileController(UserService userService, ProUserService proUserService)
        {
            _userService = userService;
            _proUserService = proUserService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetUserById()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userService.GetUserProfileAsync(userId);

            if (user == null)
            {
                return NotFound(ApiResponse<UserProfileDto>.ErrorResponse(
                    new List<string> { "User not found" }, "User not found"));
            }

            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(user, "User retrieved successfully"));
        }

        [HttpPut("update-profile")]
        public async Task<ActionResult<ApiResponse<UpdateUserProfileDto>>> UpdateProfile(
             [FromBody] UpdateUserProfileRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // G?i service update m� kh�ng d?ng t?i avatar
            var updatedUser = await _userService.UpdateProfileAsync(userId, request);

            if (updatedUser == null)
            {
                return BadRequest(ApiResponse<UpdateUserProfileDto>.ErrorResponse(
                    new List<string> { "Update failed" }, "Could not update profile"));
            }

            return Ok(ApiResponse<UpdateUserProfileDto>.SuccessResponse(updatedUser, "Profile updated successfully"));
        }




        [HttpPut("change-Password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] string newPassword)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _userService.ChangePasswordAsync(userId, newPassword);
            if (!result)
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Change password failed" }, "Could not change password"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));
        }


        [HttpPost("avatar-test")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<object>>> TestAvatarUpload(
            [FromForm] ChangeAvatarRequest request)
        {
            try
            {
                Console.WriteLine("🔍 Test Avatar Upload - Starting...");
                Console.WriteLine($"🔍 Request is null: {request == null}");
                Console.WriteLine($"🔍 Avatar file is null: {request?.Avatar == null}");
                
                if (request?.Avatar != null)
                {
                    Console.WriteLine($"🔍 File details:");
                    Console.WriteLine($"  - FileName: {request.Avatar.FileName}");
                    Console.WriteLine($"  - Length: {request.Avatar.Length}");
                    Console.WriteLine($"  - ContentType: {request.Avatar.ContentType}");
                    Console.WriteLine($"  - Headers: {string.Join(", ", request.Avatar.Headers.Select(h => $"{h.Key}={h.Value}"))}");
                }

                if (request == null || request.Avatar == null || request.Avatar.Length == 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Invalid file" }, "Avatar is required"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(new { 
                    fileName = request.Avatar.FileName,
                    length = request.Avatar.Length,
                    contentType = request.Avatar.ContentType
                }, "Test upload successful"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in TestAvatarUpload: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal server error"));
            }
        }

        [HttpPost("avatar-base64")]
        public async Task<ActionResult<ApiResponse<object>>> ChangeAvatarBase64(
            [FromBody] ChangeAvatarBase64Request request,
            [FromServices] CloudinaryService cloudinary)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                Console.WriteLine($"🔍 Base64 Avatar upload - UserId: {userId}");
                Console.WriteLine($"🔍 Base64 data length: {request.Base64Data?.Length ?? 0}");
                Console.WriteLine($"🔍 File name: {request.FileName}");
                Console.WriteLine($"🔍 Mime type: {request.MimeType}");

                if (string.IsNullOrEmpty(request.Base64Data))
                {
                    Console.WriteLine("❌ Invalid base64 data");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Invalid base64 data" }, "Base64 data is required"));
                }

                // Convert base64 to byte array
                byte[] imageBytes;
                try
                {
                    // Remove data URL prefix if present
                    string base64Data = request.Base64Data;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }
                    imageBytes = Convert.FromBase64String(base64Data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error converting base64: {ex.Message}");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Invalid base64 format" }, "Could not decode base64 data"));
                }

                // Create IFormFile from byte array
                var stream = new MemoryStream(imageBytes);
                var formFile = new FormFile(stream, 0, imageBytes.Length, "avatar", request.FileName ?? "avatar.jpg")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = request.MimeType ?? "image/jpeg"
                };

                Console.WriteLine("🔍 Starting Cloudinary upload...");
                string avatarUrl = await cloudinary.UploadFileAsync(formFile);
                
                if (string.IsNullOrEmpty(avatarUrl))
                {
                    Console.WriteLine("❌ Cloudinary upload failed");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Upload failed" }, "Could not upload avatar to cloud storage"));
                }

                Console.WriteLine($"✅ Cloudinary upload successful: {avatarUrl}");
                var result = await _userService.ChangeAvatarAsync(userId, avatarUrl);
                if (!result)
                {
                    Console.WriteLine("❌ Database update failed");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Change avatar failed" }, "Could not update avatar in database"));
                }

                Console.WriteLine("✅ Avatar update completed successfully");
                return Ok(ApiResponse<object>.SuccessResponse(new { avatarUrl }, "Avatar updated successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in ChangeAvatarBase64: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal server error"));
            }
        }

        [HttpPut("avatar-simple")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<object>>> ChangeAvatarSimple(
            IFormFile avatar,
            [FromServices] CloudinaryService cloudinary)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                Console.WriteLine($"🔍 Simple Avatar upload - UserId: {userId}");
                Console.WriteLine($"🔍 Avatar file is null: {avatar == null}");
                
                if (avatar != null)
                {
                    Console.WriteLine($"🔍 File details - Name: {avatar.FileName}, Size: {avatar.Length}, ContentType: {avatar.ContentType}");
                }

                if (avatar == null || avatar.Length == 0)
                {
                    Console.WriteLine("❌ Invalid file - avatar is null/empty");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Invalid file" }, "Avatar is required"));
                }

                Console.WriteLine("🔍 Starting Cloudinary upload...");
                string avatarUrl = await cloudinary.UploadFileAsync(avatar);
                
                if (string.IsNullOrEmpty(avatarUrl))
                {
                    Console.WriteLine("❌ Cloudinary upload failed");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Upload failed" }, "Could not upload avatar to cloud storage"));
                }

                Console.WriteLine($"✅ Cloudinary upload successful: {avatarUrl}");
                var result = await _userService.ChangeAvatarAsync(userId, avatarUrl);
                if (!result)
                {
                    Console.WriteLine("❌ Database update failed");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Change avatar failed" }, "Could not update avatar in database"));
                }

                Console.WriteLine("✅ Avatar update completed successfully");
                return Ok(ApiResponse<object>.SuccessResponse(new { avatarUrl }, "Avatar updated successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in ChangeAvatarSimple: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal server error"));
            }
        }

        [HttpPut("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<object>>> ChangeAvatar(
            [FromForm] ChangeAvatarRequest request,
            [FromServices] CloudinaryService cloudinary)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                // Debug logging
                Console.WriteLine($"🔍 Avatar upload request - UserId: {userId}");
                Console.WriteLine($"🔍 Request is null: {request == null}");
                Console.WriteLine($"🔍 Avatar file is null: {request?.Avatar == null}");
                
                if (request?.Avatar != null)
                {
                    Console.WriteLine($"🔍 File details - Name: {request.Avatar.FileName}, Size: {request.Avatar.Length}, ContentType: {request.Avatar.ContentType}");
                }

                if (request == null || request.Avatar == null || request.Avatar.Length == 0)
                {
                    Console.WriteLine("❌ Invalid file - request or avatar is null/empty");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Invalid file" }, "Avatar is required"));
                }

                // Tạm thời disable validation để test
                Console.WriteLine("🔍 Skipping validation for testing...");
                
                // Validate file type
                // var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                // if (!allowedTypes.Contains(request.Avatar.ContentType.ToLower()))
                // {
                //     Console.WriteLine($"❌ Invalid file type: {request.Avatar.ContentType}");
                //     return BadRequest(ApiResponse<object>.ErrorResponse(
                //         new List<string> { "Invalid file type" }, "Only JPEG, PNG, and GIF files are allowed"));
                // }

                // Validate file size (max 5MB)
                // if (request.Avatar.Length > 5 * 1024 * 1024)
                // {
                //     Console.WriteLine($"❌ File too large: {request.Avatar.Length} bytes");
                //     return BadRequest(ApiResponse<object>.ErrorResponse(
                //         new List<string> { "File too large" }, "File size must be less than 5MB"));
                // }

                Console.WriteLine("🔍 Starting Cloudinary upload...");
                string avatarUrl = await cloudinary.UploadFileAsync(request.Avatar);
                
                if (string.IsNullOrEmpty(avatarUrl))
                {
                    Console.WriteLine("❌ Cloudinary upload failed");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Upload failed" }, "Could not upload avatar to cloud storage"));
                }

                Console.WriteLine($"✅ Cloudinary upload successful: {avatarUrl}");
                var result = await _userService.ChangeAvatarAsync(userId, avatarUrl);
                if (!result)
                {
                    Console.WriteLine("❌ Database update failed");
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Change avatar failed" }, "Could not update avatar in database"));
                }

                Console.WriteLine("✅ Avatar update completed successfully");
                return Ok(ApiResponse<object>.SuccessResponse(new { avatarUrl }, "Avatar updated successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in ChangeAvatar: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal server error"));
            }
        }

        [HttpDelete]
        public async Task<ActionResult<ApiResponse<object>>> DeleteAccount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _userService.DeactivateAccountAsync(userId);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Deactivate account failed" }, "Could not deactivate account"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, "Account deactivated successfully"));
        }

        [HttpGet("pro-permissions")]
        public async Task<ActionResult<ApiResponse<ProUserInfo>>> GetProUserPermissions()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var proUserInfo = await _proUserService.GetProUserInfoAsync(userId);

            if (proUserInfo == null)
            {
                return NotFound(ApiResponse<ProUserInfo>.ErrorResponse(
                    new List<string> { "Pro user info not found" }, "User is not a Pro user"));
            }

            return Ok(ApiResponse<ProUserInfo>.SuccessResponse(proUserInfo, "Pro user permissions retrieved successfully"));
        }

        [HttpGet("is-pro")]
        public async Task<ActionResult<ApiResponse<bool>>> IsProUser()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isPro = await _proUserService.IsProUserAsync(userId);

            return Ok(ApiResponse<bool>.SuccessResponse(isPro, "Pro user status retrieved successfully"));
        }

        [HttpPost("create-health-profile")]
        public async Task<ActionResult<ApiResponse<object>>> CreateHealthProfile([FromBody] CreateHealthProfileRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var result = await _userService.CreateHealthProfileAsync(userId, request);
                
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Health profile created successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        new List<string> { "Failed to create health profile" }, 
                        "Create health profile failed"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Create health profile failed"));
            }
        }

        [HttpGet("debug-profile")]
        public async Task<ActionResult<ApiResponse<object>>> DebugProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            try
            {
                var result = await _userService.DebugUserProfileAsync(userId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Debug info retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Debug failed"));
            }
        }

        [HttpGet("debug-database")]
        public async Task<ActionResult<ApiResponse<object>>> DebugDatabase()
        {
            try
            {
                var dietPlans = await _userService.GetAllDietPlansAsync();
                var cookingLevels = await _userService.GetAllCookingLevelsAsync();
                var healthGoals = await _userService.GetAllHealthGoalsAsync();
                var lifestyles = await _userService.GetAllLifestylesAsync();
                
                var result = new
                {
                    dietPlans = dietPlans.Select(dp => new { id = dp.Id, name = dp.Name }),
                    cookingLevels = cookingLevels.Select(cl => new { id = cl.Id, name = cl.Name }),
                    healthGoals = healthGoals.Select(hg => new { id = hg.Id, name = hg.Name }),
                    lifestyles = lifestyles.Select(l => new { id = l.Id, name = l.Name })
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(result, "Database debug data retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Internal server error"));
            }
        }

    }
}
