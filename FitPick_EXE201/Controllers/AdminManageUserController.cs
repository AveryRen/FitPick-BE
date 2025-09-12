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

        // Lấy danh sách user
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<User>>>> GetAllUsers(
            [FromQuery] string? searchKeyword,
            [FromQuery] string? sortBy,
            [FromQuery] bool sortDesc = false,
            [FromQuery] int? genderId = null,
            [FromQuery] int? roleId = null,
            [FromQuery] bool? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentAdminId))
                return Unauthorized(ApiResponse<PagedResult<User>>.ErrorResponse(
                    new List<string> { "Unauthorized" }, "You must be logged in as Admin"));

            var usersPaged = await _userService.GetAllUsersAsync(
                currentAdminId,
                searchKeyword,
                sortBy,
                sortDesc,
                genderId,
                roleId,
                status,
                pageNumber,
                pageSize
            );

            return Ok(ApiResponse<PagedResult<User>>.SuccessResponse(usersPaged, "Users retrieved successfully"));
        }


        // Lấy thông tin user theo ID
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
         [FromForm] CreateUserRequest request,
         [FromServices] CloudinaryService cloudinary)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? avatarUrl = null;
            if (request.Avatar != null)
            {
                avatarUrl = await cloudinary.UploadFileAsync(request.Avatar);
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
                AvatarUrl = avatarUrl,
                Status = true 
            };

            var createdUser = await _userService.CreateUserAsync(newUser);

             var response = new
            {
                createdUser.Fullname,
                createdUser.Email,
                createdUser.GenderId,
                createdUser.Age,
                createdUser.Height,
                createdUser.Weight,
                createdUser.Country,
                createdUser.RoleId,
                createdUser.AvatarUrl,
                createdUser.Status
            };

            return Ok(response);
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


        // ✅ Xoá mềm (deactivate) user
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
