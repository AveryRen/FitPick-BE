using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Mvc;
using FitPick_EXE201.Helpers;

namespace FitPick_EXE201.Controllers
{
    [Route("api/meal-reviews")]
    [ApiController]
    public class MealReviewsController : ControllerBase
    {
        private readonly MealReviewService _service;

        public MealReviewsController(MealReviewService service)
        {
            _service = service;
        }

        private int GetUserIdFromToken()
        {
            return int.Parse(User.FindFirst("id")?.Value ??
                             throw new UnauthorizedAccessException("User ID not found in token"));
        }

        // Hàm map Entity -> DTO
        private MealReviewDto ToDto(Models.Entities.MealReview entity)
        {
            return new MealReviewDto
            {
                Rating = entity.Rating,
                Comment = entity.Comment,
             };
        }


        // GET: api/meal-reviews/meal/5
        [HttpGet("meal/{mealId}")]
        public async Task<IActionResult> GetMealReviews(int mealId)
        {
            var reviews = await _service.GetMealReviewsAsync(mealId);

            var dtoList = reviews.Select(r => ToDto(r)).ToList();

            return Ok(ApiResponse<IEnumerable<MealReviewDto>>.SuccessResponse(
                dtoList, "Fetched meal reviews successfully"));
        }

        // GET: api/meal-reviews/user/3/meal/5
        [HttpGet("user/{userId}/meal/{mealId}")]
        public async Task<IActionResult> GetUserReview(int userId, int mealId)
        {
            var review = await _service.GetUserReviewAsync(userId, mealId);
            if (review == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Review not found" }, "Not found"));
            }

            return Ok(ApiResponse<MealReviewDto>.SuccessResponse(
                ToDto(review), "Fetched user review successfully"));
        }

        // POST: api/meal-reviews
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] MealReviewCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(ApiResponse<string>.ErrorResponse(errors, "Invalid request"));
            }

            var userId = GetUserIdFromToken();

            var created = await _service.CreateReviewAsync(model, userId);

            return Ok(ApiResponse<MealReviewDto>.SuccessResponse(
                ToDto(created), "Review created successfully"));
        }
        // PUT: api/meal-reviews/{mealId}
        [HttpPut("{mealId}")]
        public async Task<IActionResult> UpdateReview(int mealId, [FromBody] MealReviewDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(ApiResponse<string>.ErrorResponse(errors, "Invalid request"));
            }

             var userId = GetUserIdFromToken();

            var updated = await _service.UpdateReviewAsync(userId, mealId, model);

             return Ok(ApiResponse<MealReviewDto>.SuccessResponse(
                new MealReviewDto
                {
                    Rating = updated.Rating,
                    Comment = updated.Comment
                },
                "Review updated successfully"));
        }

        // DELETE: api/meal-reviews/{mealId}
        [HttpDelete("{mealId}")]
        public async Task<IActionResult> DeleteReview(int mealId)
        {
            var userId = GetUserIdFromToken();
            await _service.DeleteReviewAsync(userId, mealId);

            return Ok(ApiResponse<string>.SuccessResponse(
                null, "Review deleted successfully"));
        }
    }
}
