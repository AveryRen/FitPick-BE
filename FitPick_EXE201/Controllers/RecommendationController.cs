using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [ApiController]
    [Route("api/recommendations")]
    [Authorize]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationService _service;

        public RecommendationController(RecommendationService service)
        {
            _service = service;
        }

        // GET /api/recommendations/meal
        [HttpGet("meal")]
        public async Task<ActionResult<List<MealRecommendationDto>>> GetMealRecommendations([FromQuery] int count = 5)
        {
            // Lấy userId từ JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
                return Unauthorized("User không hợp lệ hoặc chưa đăng nhập.");

            var meals = await _service.GetMealRecommendationsAsync(userId, count);
            return Ok(meals);
        }

        // GET /api/recommendations/ingredient?mealId=1&count=5
        [HttpGet("ingredient")]
        public async Task<ActionResult<List<IngredientRecommendationDto>>> GetIngredientRecommendations(
            [FromQuery] int mealId,
            [FromQuery] int count = 5)
        {
            if (mealId <= 0) return BadRequest("MealId không hợp lệ.");

            var ingredients = await _service.GetIngredientRecommendationsAsync(mealId, count);
            return Ok(ingredients);
        }
    }
}
