using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPick_EXE201.Controllers
{
    [Route("api/admin/ingredients")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminIngredientsController : ControllerBase
    {
        private readonly AdminIngredientService _service;

        public AdminIngredientsController(AdminIngredientService service)
        {
            _service = service;
        }

        // GET: api/admin/ingredients
        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetAll(
            [FromQuery] string? name = null,
            [FromQuery] string? type = null,
            [FromQuery] string? unit = null,
            [FromQuery] bool? status = null,
            [FromQuery] string? sortBy = "ingredientid",
            [FromQuery] bool sortDesc = true,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 0)
        {
            // Use paginated method if page and pageSize are provided
            if (page > 0 && pageSize > 0)
            {
                var (ingredients, totalCount) = await _service.GetAllPagedAsync(
                    page, pageSize, name, type, unit, status, sortBy, sortDesc);

                var result = new
                {
                    items = ingredients,
                    totalItems = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    pageSize = pageSize,
                    pageNumber = page
                };

                return Ok(ApiResponse<object>.SuccessResponse(result, "Lấy danh sách nguyên liệu thành công"));
            }
            else
            {
                // Fallback to non-paginated method for backward compatibility
                bool onlyActive = status == null ? true : status.Value;
                var ingredients = await _service.GetAllAsync(name, type, unit, onlyActive);
                return Ok(ApiResponse<IEnumerable<Ingredient>>.SuccessResponse(ingredients, "Lấy danh sách nguyên liệu thành công"));
            }
        }

        // GET: api/admin/ingredients/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<Ingredient>>> GetById(int id)
        {
            var ingredient = await _service.GetByIdAsync(id);
            if (ingredient == null)
                return NotFound(ApiResponse<Ingredient>.ErrorResponse(
                    new List<string> { "Nguyên liệu không tồn tại" }, "Không tìm thấy"));

            return Ok(ApiResponse<Ingredient>.SuccessResponse(ingredient, "Lấy nguyên liệu thành công"));
        }
        // POST: api/admin/ingredients
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Ingredient>>> Create([FromBody] IngredientCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<Ingredient>.ErrorResponse(errors, "Dữ liệu không hợp lệ"));
            }

            var created = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Ingredientid },
                ApiResponse<Ingredient>.SuccessResponse(created, "Tạo nguyên liệu thành công")
            );
        }

        // PUT: api/admin/ingredients/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<Ingredient>>> Update(int id, [FromBody] IngredientUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<Ingredient>.ErrorResponse(errors, "Dữ liệu không hợp lệ"));
            }

            var updated = await _service.UpdateAsync(id, dto);

            if (updated == null)
            {
                return NotFound(ApiResponse<Ingredient>.ErrorResponse(
                    new List<string> { "Nguyên liệu không tồn tại" }, "Không tìm thấy"));
            }

            return Ok(ApiResponse<Ingredient>.SuccessResponse(updated, "Cập nhật nguyên liệu thành công"));
        }


        // DELETE: api/admin/ingredients/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Nguyên liệu không tồn tại" }, "Không tìm thấy"));

            return Ok(ApiResponse<string>.SuccessResponse("Đã xóa thành công", "Xóa nguyên liệu thành công"));
        }
    }
}
