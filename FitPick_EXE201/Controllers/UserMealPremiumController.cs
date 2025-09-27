using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPick_EXE201.Controllers
{
    [Route("api/users/meals")]
    [ApiController]
    [Authorize(Roles = "Premium,Admin")] // Chỉ Premium hoặc Admin mới được truy cập
    public class UserMealPremiumController : ControllerBase
    {
        private readonly MealPremiumService _service;

        public UserMealPremiumController(MealPremiumService service) // Tên constructor phải trùng class
        {
            _service = service;
        }

        // GET /api/users/meals/premium
        [HttpGet("premium")]
        public async Task<ActionResult<ApiResponse<List<MealResponseDto>>>> GetPremiumMeals()
        {
            var meals = await _service.GetPremiumMealsAsync();
            return Ok(ApiResponse<List<MealResponseDto>>.SuccessResponse(meals, "Premium meals fetched successfully"));
        }

        // GET /api/users/meals/filter?CategoryId=1&MaxCalories=500&DietType=Vegan
        [HttpGet("filter")]
        public async Task<ActionResult<ApiResponse<List<MealResponseDto>>>> FilterMeals([FromQuery] MealFilterDto filter)
        {
            var meals = await _service.FilterPremiumMealsAsync(filter);
            return Ok(ApiResponse<List<MealResponseDto>>.SuccessResponse(meals, "Filtered premium meals fetched successfully"));
        }
    }
}
