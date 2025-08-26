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
        public async Task<ActionResult<ApiResponse<IEnumerable<Ingredient>>>> GetAll(
            [FromQuery] string? name = null,
            [FromQuery] string? type = null,
            [FromQuery] string? unit = null,
            [FromQuery] bool onlyActive = true)
        {
            var ingredients = await _service.GetAllAsync(name, type, unit, onlyActive);
            return Ok(ApiResponse<IEnumerable<Ingredient>>.SuccessResponse(ingredients, "Lấy danh sách nguyên liệu thành công"));
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
