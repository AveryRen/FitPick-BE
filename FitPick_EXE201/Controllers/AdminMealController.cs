using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/admin/meals")]
    [ApiController]
    public class AdminMealController : ControllerBase
    {
        private readonly AdminMealService _mealService;

        public AdminMealController(AdminMealService mealService)
        {
            _mealService = mealService;
        }
        private int GetUserIdFromToken()
        {
            return int.Parse(User.FindFirst("id")?.Value ??
                             throw new UnauthorizedAccessException("User ID not found in token"));
        }

        // GET: api/admin/meals
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? categoryId,
            [FromQuery] int? minCalories,
            [FromQuery] int? maxCalories,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var meals = await _mealService.GetAllAsync(categoryId, minCalories, maxCalories, minPrice, maxPrice);

            return Ok(ApiResponse<IEnumerable<Meal>>.SuccessResponse(meals, "Lấy danh sách meal thành công"));
        }

        // GET: api/admin/meals/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var meal = await _mealService.GetByIdAsync(id);
            if (meal == null)
                return NotFound(ApiResponse<Meal>.ErrorResponse(
                    new List<string> { "Meal không tồn tại" }, "Không tìm thấy meal"));

            return Ok(ApiResponse<Meal>.SuccessResponse(meal, "Lấy meal thành công"));
        }

        // POST: api/admin/meals
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MealCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<Meal>.ErrorResponse(errors, "Dữ liệu không hợp lệ"));
            }

            // Mapping từ DTO sang Entity
            var meal = new Meal
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                Calories = dto.Calories,
                Cookingtime = dto.Cookingtime,
                Diettype = dto.Diettype,
                Price = dto.Price,
                StatusId = dto.StatusId ?? 2, // default "Pending" = 2
                Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                Createdby = GetUserIdFromToken() // ví dụ lấy từ token/session
            };

            var createdMeal = await _mealService.AddAsync(meal);
            return Ok(ApiResponse<Meal>.SuccessResponse(createdMeal, "Tạo meal thành công"));
        }

        // PUT: api/admin/meals/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MealUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<MealUpdateDto>.ErrorResponse(errors, "Dữ liệu không hợp lệ"));
            }

            var meal = await _mealService.GetByIdAsync(id);
            if (meal == null)
            {
                return NotFound(ApiResponse<MealUpdateDto>.ErrorResponse(
                    new List<string> { "Meal không tồn tại" }, "Không tìm thấy meal"));
            }

            // Map dữ liệu từ DTO sang entity
            meal.Name = dto.Name;
            meal.Description = dto.Description;
            meal.Calories = dto.Calories;
            meal.Cookingtime = dto.Cookingtime;
            meal.CategoryId = dto.CategoryId;
            meal.Diettype = dto.Diettype;
            meal.Price = dto.Price;
            meal.StatusId = dto.StatusId;

            var updatedMeal = await _mealService.UpdateAsync(meal);

            return Ok(ApiResponse<Meal>.SuccessResponse(updatedMeal, "Cập nhật meal thành công"));
        }


        // DELETE: api/admin/meals/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mealService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Meal không tồn tại" }, "Không tìm thấy meal"));

            return Ok(ApiResponse<string>.SuccessResponse("OK", "Xóa meal thành công"));
        }
    }
} 