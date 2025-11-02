    using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/admin/meals")]
    [ApiController]
    public class AdminMealController : ControllerBase
    {
        private readonly AdminMealService _mealService;
        private readonly CloudinaryService _cloudinaryService;

        public AdminMealController(AdminMealService mealService, CloudinaryService cloudinaryService)
        {
            _mealService = mealService;
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
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
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? dietType = null,
            [FromQuery] int? statusId = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "createdat",
            [FromQuery] bool sortDesc = true,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 0)
        {
            // Use paginated method if page and pageSize are provided
            if (page > 0 && pageSize > 0)
            {
                var (meals, totalCount) = await _mealService.GetAllPagedAsync(
                    page, pageSize, categoryId, dietType, statusId, search, sortBy, sortDesc);

                var result = new
                {
                    items = meals,
                    totalItems = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    pageSize = pageSize,
                    pageNumber = page
                };

                return Ok(ApiResponse<object>.SuccessResponse(result, "Lấy danh sách meal thành công"));
            }
            else
            {
                var meals = await _mealService.GetAllAsync(categoryId, minCalories, maxCalories, minPrice, maxPrice);
                return Ok(ApiResponse<IEnumerable<Meal>>.SuccessResponse(meals, "Lấy danh sách meal thành công"));
            }
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

            var meal = new Meal
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                Calories = dto.Calories,
                Cookingtime = dto.Cookingtime,
                Diettype = dto.Diettype,
                Price = dto.Price,
                StatusId = dto.StatusId ?? 2,
                ImageUrl = dto.ImageUrl,
                Protein = dto.Protein,
                Carbs = dto.Carbs,
                Fat = dto.Fat,
                Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
            };

            if (dto.Instructions != null && dto.Instructions.Any())
            {
                meal.MealInstructions = dto.Instructions.Select((step, index) => new MealInstruction
                {
                    InstructionId = 0, // Let database generate ID
                    StepNumber = index + 1,
                    Instruction = step
                }).ToList();
            }

            var createdMeal = await _mealService.AddAsync(meal);

            if (dto.Ingredients != null && dto.Ingredients.Any())
            {
                await _mealService.AddIngredientsAsync(createdMeal.Mealid, dto.Ingredients);
            }

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
            meal.ImageUrl = dto.ImageUrl;
            meal.Protein = dto.Protein;
            meal.Carbs = dto.Carbs;
            meal.Fat = dto.Fat;

            // Xử lý Instructions
            if (dto.Instructions != null)
            {
                meal.MealInstructions.Clear();
                int step = 1;
                foreach (var ins in dto.Instructions)
                {
                    meal.MealInstructions.Add(new MealInstruction
                    {
                        InstructionId = 0,
                        StepNumber = step++,
                        Instruction = ins,
                        MealId = meal.Mealid
                    });
                }
            }

            // Xử lý Ingredients
            if (dto.Ingredients != null)
            {
                await _mealService.RemoveIngredientsAsync(meal.Mealid);
                if (dto.Ingredients.Any())
                {
                    await _mealService.AddIngredientsAsync(meal.Mealid, dto.Ingredients);
                }
            }

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

        [HttpPut("{id}/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMealImage(int id, [FromForm] MealImageUpdateDto dto)
        {
            if (!ModelState.IsValid || dto.File == null || dto.File.Length == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { "File ảnh không hợp lệ" },
                    "Cập nhật ảnh thất bại"
                ));
            }

            // Upload ảnh lên Cloudinary
            var url = await _cloudinaryService.UploadFileAsync(dto.File);
            if (string.IsNullOrEmpty(url))
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    new List<string> { "Upload ảnh thất bại" },
                    "Cập nhật ảnh thất bại"
                ));
            } 
            var updatedMeal = await _mealService.UpdateImageAsync(id, url);
            if (updatedMeal == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Meal không tồn tại" },
                    "Không tìm thấy meal"
                ));
            } 
            var responseDto = new MealImageResponseDto
            {
                MealId = updatedMeal.Mealid,
                ImageUrl = updatedMeal.ImageUrl
            };

            return Ok(ApiResponse<MealImageResponseDto>.SuccessResponse(responseDto, "Cập nhật ảnh món ăn thành công"));
        }

    }
}
