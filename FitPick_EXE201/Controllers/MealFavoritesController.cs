using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Services;
using FitPick_EXE201.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitPick_EXE201.Models.DTOs;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    [Authorize]
    public class MealFavoritesController : ControllerBase
    {
        private readonly MealReviewService _mealReviewService;

        public MealFavoritesController(MealReviewService mealReviewService)
        {
            _mealReviewService = mealReviewService;
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId) || userId == 0)
            {
                return null;
            }
            return userId;
        }

        // GET: api/favorites
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<FavoriteMealDto>>>> GetFavorites()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(ApiResponse<IEnumerable<FavoriteMealDto>>.ErrorResponse(
                        new List<string> { "User ID not found in token" }, "Unauthorized"));

                var favorites = await _mealReviewService.GetUserFavoritesAsync(userId.Value);

                // Map entity -> DTO (n?u chua d�ng AutoMapper)
                var dtoList = favorites.Select(f => new FavoriteMealDto
                {
                    MealId = f.Mealid,
                    MealName = f.Meal?.Name ?? string.Empty,
                    IsFavorite = true, // v� trong danh s�ch favorites th� m?c d?nh true
                    Rating = f.Rating,
                    Comment = f.Comment,
                    UpdatedAt = f.Updatedat
                });

                return Ok(ApiResponse<IEnumerable<FavoriteMealDto>>
                    .SuccessResponse(dtoList, "Fetched user favorites successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<FavoriteMealDto>>
                    .ErrorResponse(new List<string> { ex.Message }, "Failed to fetch favorites."));
            }
        }


        // POST: api/favorites/{mealId}
        [HttpPost("{mealId}")]
        public async Task<ActionResult<ApiResponse<string>>> AddFavorite(int mealId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(ApiResponse<string>.ErrorResponse(
                        new List<string> { "User ID not found in token" }, "Unauthorized"));

                var result = await _mealReviewService.AddFavoriteAsync(userId.Value, mealId);

                if (!result)
                {
                    return BadRequest(ApiResponse<string>
                        .ErrorResponse(new List<string> { "Could not add favorite." }, "Add favorite failed."));
                }

                return Ok(ApiResponse<string>
                    .SuccessResponse("Favorite added.", "Favorite added successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>
                    .ErrorResponse(new List<string> { ex.Message }, "Add favorite failed."));
            }
        }

        // DELETE: api/favorites/{mealId}
        [HttpDelete("{mealId}")]
        public async Task<ActionResult<ApiResponse<string>>> RemoveFavorite(int mealId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized(ApiResponse<string>.ErrorResponse(
                        new List<string> { "User ID not found in token" }, "Unauthorized"));

                var result = await _mealReviewService.RemoveFavoriteAsync(userId.Value, mealId);

                if (!result)
                {
                    return BadRequest(ApiResponse<string>
                        .ErrorResponse(new List<string> { "Could not remove favorite." }, "Remove favorite failed."));
                }

                return Ok(ApiResponse<string>
                    .SuccessResponse("Favorite removed.", "Favorite removed successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>
                    .ErrorResponse(new List<string> { ex.Message }, "Remove favorite failed."));
            }
        }
    }
}
