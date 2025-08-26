using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/user-ingredients")]
[ApiController]
[Authorize(Roles = "User,Premium,Admin")]
public class UserIngredientsController : ControllerBase
{
    private readonly UserIngredientService _service;
    private readonly AdminIngredientService _adminService;

    public UserIngredientsController(UserIngredientService service, AdminIngredientService adminService)
    {
        _service = service;
        _adminService = adminService;
    }

    private int GetUserIdFromToken()
    {
        return int.Parse(User.FindFirst("id")?.Value ??
                         throw new UnauthorizedAccessException("User ID not found in token"));
    }

    // GET: api/user-ingredients/available
    [HttpGet("available")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Ingredient>>>> GetAvailableIngredients(
        [FromQuery] string? name = null,
        [FromQuery] string? type = null,
        [FromQuery] string? unit = null,
        [FromQuery] bool onlyActive = true)
    {
        var ingredients = await _adminService.GetAllAsync(name, type, unit, onlyActive);
        return Ok(ApiResponse<IEnumerable<Ingredient>>.SuccessResponse(ingredients, "Lấy danh sách nguyên liệu thành công"));
    }

    // GET: api/user-ingredients
    [HttpGet]
    public async Task<IActionResult> GetMyIngredients()
    {
        var userId = GetUserIdFromToken();
        var items = await _service.GetByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<UserIngredient>>.SuccessResponse(items, "Lấy nguyên liệu thành công"));
    }

    // GET: api/user-ingredients/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserIdFromToken();
        var item = await _service.GetByIdAsync(id);

        if (item == null || item.Userid != userId)
        {
            return NotFound(ApiResponse<UserIngredient>.ErrorResponse(
                new List<string> { "Không tìm thấy nguyên liệu" }, "Lỗi"));
        }

        return Ok(ApiResponse<UserIngredient>.SuccessResponse(item, "Lấy nguyên liệu thành công"));
    }

    // POST: api/user-ingredients
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateUserIngredientRequest request)
    {
        if (request == null || request.IngredientId <= 0 || request.Quantity < 0)
            return BadRequest(ApiResponse<UserIngredient>.ErrorResponse(
                new List<string> { "Dữ liệu không hợp lệ" }, "Lỗi"));

        var userId = GetUserIdFromToken();

        try
        {
            // Lấy hoặc tạo entity mới (service kiểm tra duplicate)
            var userIngredient = await _service.AddOrGetAsync(request.IngredientId, userId);

            // Cập nhật quantity nếu khác
            if (userIngredient.Quantity != request.Quantity)
            {
                userIngredient.Quantity = request.Quantity;
                userIngredient = await _service.UpdateAsync(userIngredient);
            }

            return Ok(ApiResponse<UserIngredient>.SuccessResponse(userIngredient, "Thêm nguyên liệu thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<UserIngredient>.ErrorResponse(new List<string> { ex.Message }, "Lỗi"));
        }
        catch
        {
            return StatusCode(500, ApiResponse<UserIngredient>.ErrorResponse(
                new List<string> { "Có lỗi xảy ra khi thêm nguyên liệu" }, "Lỗi"));
        }
    }

    // PUT: api/user-ingredients/{id}
    [HttpPut("{id}/quantity")]
    public async Task<IActionResult> UpdateQuantity(int id, [FromBody]
    [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue)] decimal quantity)
    {
        var userId = GetUserIdFromToken();
        var entity = await _service.GetByIdAsync(id);
        if (entity == null || entity.Userid != userId)
            return NotFound(ApiResponse<bool>.ErrorResponse(
                new List<string> { "Không tìm thấy nguyên liệu" }, "Lỗi"));

        entity.Quantity = quantity;
        var updated = await _service.UpdateAsync(entity);
        return Ok(ApiResponse<UserIngredient>.SuccessResponse(updated, "Cập nhật quantity thành công"));
    }

    // PATCH: api/user-ingredients/{id}/reset
    [HttpPatch("{id}/reset")]
    public async Task<IActionResult> ResetQuantity(int id)
    {
        var userId = GetUserIdFromToken();
        var item = await _service.GetByIdAsync(id);

        if (item == null || item.Userid != userId)
            return NotFound(ApiResponse<bool>.ErrorResponse(
                new List<string> { "Không tìm thấy nguyên liệu" }, "Lỗi"));

        var result = await _service.ResetQuantityAsync(id);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Reset quantity thành công"));
    }
}
