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

        // Tạo user mới
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request data");

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
                RoleId = request.RoleId,
                Createdat = DateTime.Now,
                Updatedat = DateTime.Now
            };

            var created = await _userService.CreateUserAsync(newUser);

            return CreatedAtAction(nameof(GetUserById), new { id = created.Userid }, created);
        }


        // Cập nhật user
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateUser(int id, [FromBody] AdminUserDetailDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Request body cannot be null" }, "Update failed"));

            var success = await _userService.UpdateUserAsync(id, dto);

            if (!success)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { $"User with ID {id} not found" }, "Update failed"));

            return Ok(ApiResponse<string>.SuccessResponse(null, "User updated successfully"));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteUser(int id)
        {
            var user = await _userService.GetUserByIdForAdminAsync(id);
            if (user == null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { $"User with ID {id} does not exist" }, "Delete failed"));
            }

            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    new List<string> { "Server error while deleting user" }, "Delete failed"));
            }

            return Ok(ApiResponse<string>.SuccessResponse(null, $"User with ID {id} has been deleted successfully"));
        }
    }
}
