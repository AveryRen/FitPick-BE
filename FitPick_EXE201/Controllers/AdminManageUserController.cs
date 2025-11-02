using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;
using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminManageUserController : ControllerBase
    {
        private readonly AdminManageUserService _userService;

        public AdminManageUserController(AdminManageUserService userService)
        {
            _userService = userService;
        }

        // L?y danh s�ch user
        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetAllUsers(
    [FromQuery] string? searchKeyword,
    [FromQuery] string? sortBy,
    [FromQuery] bool sortDesc = false,
    [FromQuery] int? genderId = null,
    [FromQuery] int? roleId = null,
    [FromQuery] bool? status = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10
)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentAdminId))
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Unauthorized" }, "You must be logged in as Admin"));

            // Use paginated method if page and pageSize are provided
            if (page > 0 && pageSize > 0)
            {
                var (users, totalCount) = await _userService.GetAllUsersPagedAsync(
                    currentAdminId,
                    searchKeyword,
                    sortBy,
                    sortDesc,
                    genderId,
                    roleId,
                    status,
                    page,
                    pageSize
                );

                var result = new
                {
                    items = users,
                    totalItems = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    pageSize = pageSize,
                    pageNumber = page
                };

                return Ok(ApiResponse<object>.SuccessResponse(result, "Users retrieved successfully"));
            }
            else
            {
                // Fallback to non-paginated for backward compatibility
                var users = await _userService.GetAllUsersAsync(
                    currentAdminId,
                    searchKeyword,
                    sortBy,
                    sortDesc,
                    genderId,
                    roleId,
                    status
                );

                return Ok(ApiResponse<List<User>>.SuccessResponse(users, "Users retrieved successfully"));
            }
        }



        // L?y th�ng tin user theo ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AdminUserDetailDto>>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdForAdminAsync(id);
            if (user is null)
                return NotFound(ApiResponse<AdminUserDetailDto>.ErrorResponse(
                    new List<string> { $"User with ID {id} not found" }, "Not Found"));

            return Ok(ApiResponse<AdminUserDetailDto>.SuccessResponse(user, "User retrieved successfully"));
        }
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<object>.ErrorResponse(errors, "D? li?u kh�ng h?p l?"));
            }

            var newUser = new User
            {
                Fullname = request.FullName,
                Email = request.Email,
                Passwordhash = request.Password,
                GenderId = request.GenderId,
                Age = request.Age,
                Height = request.Height,
                Weight = request.Weight,
                Country = request.Country,
                RoleId = request.RoleId,
                Status = true
            };

            var createdUser = await _userService.CreateUserAsync(newUser);

            var responseData = new
            {
                createdUser.Userid,
                createdUser.Fullname,
                createdUser.Email,
                createdUser.GenderId,
                createdUser.Age,
                createdUser.Height,
                createdUser.Weight,
                createdUser.Country,
                createdUser.RoleId,
                createdUser.Status
            };

            return Ok(ApiResponse<object>.SuccessResponse(responseData, "T?o ngu?i d�ng th�nh c�ng"));
        }
        [HttpPut("{id}/avatar")]
        public async Task<IActionResult> UpdateAvatar(
           int id,
           [FromForm] UpdateAvatarUserRequestDto request,
           [FromServices] CloudinaryService cloudinary)
        {
            if (request.Avatar == null || request.Avatar.Length == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Kh�ng c� file d? upload." },
                    "C?p nh?t ?nh th?t b?i"
                ));
            }

            // Upload ?nh
            var avatarUrl = await cloudinary.UploadFileAsync(request.Avatar);
            if (string.IsNullOrEmpty(avatarUrl))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Upload ?nh th?t b?i." },
                    "Kh�ng th? luu ?nh"
                ));
            }

            // C?p nh?t DB
            var updatedUser = await _userService.UpdateUserAvatarAsync(id, avatarUrl);
            if (updatedUser == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Kh�ng t�m th?y user v?i id = {id}" },
                    "Ngu?i d�ng kh�ng t?n t?i"
                ));
            }

            var responseDto = new UserAvatarResponseDto
            {
                Userid = updatedUser.Userid,
                Fullname = updatedUser.Fullname,
                Email = updatedUser.Email,
                AvatarUrl = updatedUser.AvatarUrl
            };

            return Ok(ApiResponse<UserAvatarResponseDto>.SuccessResponse(
                responseDto,
                "C?p nh?t ?nh d?i di?n th�nh c�ng"
            ));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateUser(
             int id,
             [FromForm] UpdateUserRequest request,
             [FromServices] CloudinaryService cloudinary)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    new List<string> { "Invalid request data" }, "Bad Request"));

            string? avatarUrl = null;
            if (request.Avatar != null)
            {
                avatarUrl = await cloudinary.UploadFileAsync(request.Avatar);
            }

            var dto = new AdminUserDetailDto
            {
                Fullname = request.Fullname,
                Email = request.Email,
                GenderId = request.GenderId ?? 0,
                Age = request.Age,
                Height = request.Height,
                Weight = request.Weight,
                Country = request.Country,
                City = request.City,
                RoleId = request.RoleId ?? 0,
                Status = request.Status ?? true,
                AvatarUrl = avatarUrl
            };

            var success = await _userService.UpdateUserAsync(id, dto);

            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(
                    new List<string> { $"User with ID {id} not found" }, "Not Found"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "User updated successfully"));
        }


        // ? Xo� m?m (deactivate) user
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(
                    new List<string> { $"User with ID {id} not found" }, "Not Found"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "User deleted successfully"));
        }

        [HttpPut("{id}/change-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(
            int id,
            [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    new List<string> { "New password is required" }, "Bad Request"));
            }

            var success = await _userService.ChangePasswordAsync(id, newPassword);

            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(
                    new List<string> { $"User with ID {id} not found" }, "Not Found"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Password changed successfully"));
        }

    }
}
