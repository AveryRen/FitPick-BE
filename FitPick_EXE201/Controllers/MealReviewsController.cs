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
                ReviewId = entity.Reviewid,
                Rating = entity.Rating,
                Comment = entity.Comment,
                UserName = entity.User?.Fullname ?? "Anonymous",
                UserAvatar = entity.User?.AvatarUrl,
                CreatedAt = entity.Createdat,
                UpdatedAt = entity.Updatedat
            };
        }

        // GET: api/meal-reviews/meal/5
        [HttpGet("meal/{mealId}")]
        public async Task<IActionResult> GetMealReviews(int mealId)
        {
            try
            {
                if (mealId <= 0)
                    return BadRequest(ApiResponse<IEnumerable<MealReviewDto>>.ErrorResponse(
                        new List<string> { "Invalid meal ID" }, "Bad Request"));

                var reviews = await _service.GetMealReviewsAsync(mealId);
                var dtoList = reviews.Select(r => ToDto(r)).ToList();

                return Ok(ApiResponse<IEnumerable<MealReviewDto>>.SuccessResponse(
                    dtoList, "Fetched meal reviews successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<MealReviewDto>>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal Server Error"));
            }
        }

        // GET: api/meal-reviews/meal/5/paginated?page=1&pageSize=10
        [HttpGet("meal/{mealId}/paginated")]
        public async Task<IActionResult> GetMealReviewsPaginated(int mealId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (mealId <= 0)
                    return BadRequest(ApiResponse<PagedResult<MealReviewDto>>.ErrorResponse(
                        new List<string> { "Invalid meal ID" }, "Bad Request"));

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var pagedResult = await _service.GetMealReviewsPaginatedAsync(mealId, page, pageSize);
                var dtoList = pagedResult.Data.Select(r => ToDto(r)).ToList();

                var pagedDtoResult = new PagedResult<MealReviewDto>
                {
                    Data = dtoList,
                    TotalCount = pagedResult.TotalCount,
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize
                };

                return Ok(ApiResponse<PagedResult<MealReviewDto>>.SuccessResponse(
                    pagedDtoResult, "Fetched paginated meal reviews successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<MealReviewDto>>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal Server Error"));
            }
        }

        // GET: api/meal-reviews/meal/5/stats
        [HttpGet("meal/{mealId}/stats")]
        public async Task<IActionResult> GetMealRatingStats(int mealId)
        {
            try
            {
                if (mealId <= 0)
                    return BadRequest(ApiResponse<MealRatingStatsDto>.ErrorResponse(
                        new List<string> { "Invalid meal ID" }, "Bad Request"));

                var stats = await _service.GetMealRatingStatsAsync(mealId);

                return Ok(ApiResponse<MealRatingStatsDto>.SuccessResponse(
                    stats, "Fetched meal rating stats successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MealRatingStatsDto>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal Server Error"));
            }
        }

        // GET: api/meal-reviews/user/3/meal/5
        [HttpGet("user/{userId}/meal/{mealId}")]
        public async Task<IActionResult> GetUserReview(int userId, int mealId)
        {
            try
            {
                if (userId <= 0 || mealId <= 0)
                    return BadRequest(ApiResponse<MealReviewDto>.ErrorResponse(
                        new List<string> { "Invalid user ID or meal ID" }, "Bad Request"));

                var review = await _service.GetUserReviewAsync(userId, mealId);
                if (review == null)
                {
                    return NotFound(ApiResponse<MealReviewDto>.ErrorResponse(
                        new List<string> { "Review not found" }, "Not found"));
                }

                return Ok(ApiResponse<MealReviewDto>.SuccessResponse(
                    ToDto(review), "Fetched user review successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MealReviewDto>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal Server Error"));
            }
        }

        // POST: api/meal-reviews
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] MealReviewCreateDto model)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MealReviewDto>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal Server Error"));
            }
        }

        // PUT: api/meal-reviews/{mealId}
        [HttpPut("{mealId}")]
        public async Task<IActionResult> UpdateReview(int mealId, [FromBody] MealReviewDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(ApiResponse<string>.ErrorResponse(errors, "Invalid request"));
                }

                if (mealId <= 0)
                    return BadRequest(ApiResponse<MealReviewDto>.ErrorResponse(
                        new List<string> { "Invalid meal ID" }, "Bad Request"));

                var userId = GetUserIdFromToken();
                var updated = await _service.UpdateReviewAsync(userId, mealId, model);

                return Ok(ApiResponse<MealReviewDto>.SuccessResponse(
                    ToDto(updated), "Review updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MealReviewDto>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal Server Error"));
            }
        }

        // DELETE: api/meal-reviews/{mealId}
        [HttpDelete("{mealId}")]
        public async Task<IActionResult> DeleteReview(int mealId)
        {
            try
            {
                if (mealId <= 0)
                    return BadRequest(ApiResponse<string>.ErrorResponse(
                        new List<string> { "Invalid meal ID" }, "Bad Request"));

                var userId = GetUserIdFromToken();
                await _service.DeleteReviewAsync(userId, mealId);

                return Ok(ApiResponse<string>.SuccessResponse(
                    null, "Review deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    new List<string> { ex.Message }, "Internal Server Error"));
            }
        }
    }
}