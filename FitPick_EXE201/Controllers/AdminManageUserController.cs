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
        public async Task<ActionResult<ApiResponse<IEnumerable<User>>>> GetAllUsers(
            [FromQuery] string? searchKeyword,
            [FromQuery] string? sortBy,
            [FromQuery] bool sortDesc = false,
            [FromQuery] int? genderId = null,
            [FromQuery] int? roleId = null,
            [FromQuery] bool? status = null
        )
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentAdminId))
                return Unauthorized(ApiResponse<IEnumerable<User>>.ErrorResponse(
                    new List<string> { "Unauthorized" }, "You must be logged in as Admin"));

            var users = await _userService.GetAllUsersAsync(
                currentAdminId,
                searchKeyword,
                sortBy,
                sortDesc,
                genderId,
                roleId,
                status
            );

            return Ok(ApiResponse<IEnumerable<User>>.SuccessResponse(users, "Users retrieved successfully"));
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
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
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
                    City = request.City,
                    RoleId = request.RoleId
                };

                var createdUser = await _userService.CreateUserAsync(newUser);

                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Userid }, new
                {
                    createdUser.Userid,
                    createdUser.Fullname,
                    createdUser.Email,
                    createdUser.RoleId,
                    createdUser.Status
                });
            }
            catch (InvalidOperationException ex) // Email đã tồn tại
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the user.",
                    detail = ex.Message
                });
            }
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateUser(int id, [FromBody] AdminUserDetailDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    new List<string> { "Invalid request data" }, "Bad Request"));

            var result = await _userService.UpdateUserAsync(id, dto);

            if (!result)
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
    }
}
