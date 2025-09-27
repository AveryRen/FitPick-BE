using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FitPick_EXE201.Services;
using FitPick_EXE201.Helpers;
using System.Collections.Generic;
using FitPick_EXE201.Models.DTOs; // ✅ dùng DTO
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace FitPick_EXE201.Controllers
{
    [Route("api/users/meals")]
    [ApiController]
    [Authorize]
    public class UserMealController : ControllerBase
    {
        private readonly UserMealService _userMealService;

        public UserMealController(UserMealService userMealService)
        {
            _userMealService = userMealService;
        }

        // GET: api/users/meals
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<MealDto>>>> GetMeals(
            [FromQuery] string? name,
            [FromQuery] int? categoryId,
            [FromQuery] string? dietType,
            [FromQuery] int? minCalories,
            [FromQuery] int? maxCalories,
            [FromQuery] int? minCookingTime,
            [FromQuery] int? maxCookingTime,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice
        )
        {
            var meals = await _userMealService.GetMealsAsync(
                name, categoryId, dietType,
                minCalories, maxCalories,
                minCookingTime, maxCookingTime,
                minPrice, maxPrice, 1
            );

            if (meals == null || !meals.Any())
            {
                return Ok(ApiResponse<IEnumerable<MealDto>>.ErrorResponse(
                    new List<string> { "No meals found" },
                    "No data"
                ));
            }

            return Ok(ApiResponse<IEnumerable<MealDto>>.SuccessResponse(meals, "Meals retrieved successfully"));
        }

        // GET: api/users/meals/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MealDto>>> GetMealById(int id)
        {
            var meal = await _userMealService.GetMealByIdAsync(id);

            if (meal == null)
            {
                return Ok(ApiResponse<MealDto>.ErrorResponse(
                    new List<string> { $"Meal with id {id} not found" },
                    "Not Found"
                ));
            }

            return Ok(ApiResponse<MealDto>.SuccessResponse(meal, "Meal retrieved successfully"));
        }
    }
}
