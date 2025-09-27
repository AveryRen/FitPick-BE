using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/user_meal_ingredient_marks")]
    [ApiController]
    [Authorize]
    public class UserMealIngredientController : ControllerBase
    {
        private readonly UserMealIngredientService _service;

        public UserMealIngredientController(UserMealIngredientService service)
        {
            _service = service;
        }

        [HttpGet("{mealId}")]
        public async Task<IActionResult> GetUserMealIngredients(int mealId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var data = await _service.GetUserMealIngredientsAsync(userId, mealId);
                
                return Ok(ApiResponse<List<UserMealIngredientDto>>
                    .SuccessResponse(data, "Lấy danh sách nguyên liệu thành công"));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<MealIngredientDto>>
                    .ErrorResponse(new List<string> { ex.Message }, "Không thể lấy danh sách nguyên liệu"));
            }
        }

        [HttpPost("mark")]
        public async Task<IActionResult> MarkIngredient([FromBody] UserMealIngredientMarkDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _service.MarkIngredientAsync(userId, dto.MealId, dto.IngredientId, dto.HasIt);

                return Ok(ApiResponse<object>
                    .SuccessResponse(null, "Cập nhật nguyên liệu thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>
                    .ErrorResponse(new List<string> { ex.Message }, "Không thể cập nhật nguyên liệu"));
            }
        }
    }
}