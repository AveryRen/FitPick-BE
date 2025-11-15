using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Services;
using FitPick_EXE201.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FitPick_EXE201.Models.DTOs;
using Org.BouncyCastle.Utilities.Collections;

namespace FitPick_EXE201.Controllers
{
    [Route("api/meal-histories")]
    [ApiController]
    [Authorize]
    public class MealHistoriesController : ControllerBase
    {
        private readonly MealHistoryService _service;

        public MealHistoriesController(MealHistoryService service)
        {
            _service = service;
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId) || userId == 0)
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }


        // GET: api/meal-histories
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<MealHistoryDto>>>> GetAllByUser()
        {
            var userId = GetUserIdFromToken();
            var histories = await _service.GetUserHistoryAsync(userId);

            // Map sang DTO, bao g?m c�c object li�n quan
            var dtoList = histories.Select(h => new MealHistoryDto
            {
                Historyid = h.Historyid,
                Mealid = h.Mealid,
                MealtimeId = h.MealtimeId,
                Date = h.Date,
                Quantity = h.Quantity,
                Unit = h.Unit,
                Calories = h.Calories,
                Createdat = h.Createdat,

                Meal = h.Meal == null ? null : new
                {
                    h.Meal.Mealid,
                    h.Meal.Name,
                    h.Meal.Calories,
                    h.Meal.ImageUrl
                },
                Mealtime = h.Mealtime == null ? null : new
                {
                    h.Mealtime.Id,
                    h.Mealtime.Name
                }
            }).ToList();

            return Ok(ApiResponse<IEnumerable<MealHistoryDto>>.SuccessResponse(
                dtoList,
                "Fetched meal history successfully"
            ));
        }


        // POST: api/meal-histories
        [HttpPost]
        public async Task<ActionResult<ApiResponse<MealHistoryResponseDto>>> Create(CreateMealHistoryDto dto)
        {
            var userId = GetUserIdFromToken();

            // T?o entity t? DTO request
            var history = new MealHistory
            {
                Userid = userId,
                Mealid = dto.Mealid,
                MealtimeId = dto.MealtimeId,
                Date = dto.Date,
                Quantity = dto.Quantity,
                Unit = dto.Unit,
                Calories = dto.Calories,
                Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
            };

            await _service.AddMealHistoryAsync(history);

            // Map entity sang DTO response
            var responseDto = new MealHistoryResponseDto
            {
                Historyid = history.Historyid,
                Mealid = history.Mealid,
                MealtimeId = history.MealtimeId,
                Date = history.Date,
                Quantity = history.Quantity,
                Unit = history.Unit,
                Calories = history.Calories
            };

            return Ok(ApiResponse<MealHistoryResponseDto>.SuccessResponse(
                responseDto,
                "Meal history created successfully"
            ));
        }


        // DELETE: api/meal-histories/10
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var userId = GetUserIdFromToken();

            // (t�y b?n c� mu?n check ownership kh�ng)
            var success = await _service.DeleteMealHistoryAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Meal history not found" },
                    "Not Found"
                ));
            }

            return Ok(ApiResponse<string>.SuccessResponse(
                "Deleted",
                "Meal history deleted successfully"
            ));
        }

        // GET: api/meal-histories/by-date?date=2025-01-15
        [HttpGet("by-date")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MealHistoryDto>>>> GetByDate([FromQuery] DateOnly date)
        {
            var userId = GetUserIdFromToken();
            var histories = await _service.GetUserHistoryByDateAsync(userId, date);

            var dtoList = histories.Select(h => new MealHistoryDto
            {
                Historyid = h.Historyid,
                Mealid = h.Mealid,
                MealtimeId = h.MealtimeId,
                Date = h.Date,
                Quantity = h.Quantity,
                Unit = h.Unit,
                Calories = h.Calories,
                Createdat = h.Createdat,
                Meal = h.Meal == null ? null : new
                {
                    h.Meal.Mealid,
                    h.Meal.Name,
                    h.Meal.Calories,
                    h.Meal.Protein,
                    h.Meal.Carbs,
                    h.Meal.Fat,
                    h.Meal.ImageUrl
                },
                Mealtime = h.Mealtime == null ? null : new
                {
                    h.Mealtime.Id,
                    h.Mealtime.Name
                }
            }).ToList();

            return Ok(ApiResponse<IEnumerable<MealHistoryDto>>.SuccessResponse(
                dtoList,
                "Fetched meal history by date successfully"
            ));
        }

        // GET: api/meal-histories/detailed-stats?date=2025-01-15
        [HttpGet("detailed-stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetDetailedStats([FromQuery] DateOnly date)
        {
            var userId = GetUserIdFromToken();
            var stats = await _service.GetDetailedDailyStatsAsync(userId, date);

            return Ok(ApiResponse<object>.SuccessResponse(
                stats,
                "Fetched detailed daily nutrition stats successfully"
            ));
        }

        // GET: api/meal-histories/check-eaten?mealId=123&date=2025-01-15
        [HttpGet("check-eaten")]
        public async Task<ActionResult<ApiResponse<object>>> CheckMealEaten([FromQuery] int mealId, [FromQuery] DateOnly date)
        {
            var userId = GetUserIdFromToken();
            var isEaten = await _service.IsMealEatenTodayAsync(userId, mealId, date);
            var mealHistory = await _service.GetMealHistoryByMealAndDateAsync(userId, mealId, date);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { IsEaten = isEaten, MealHistory = mealHistory },
                "Checked meal eaten status successfully"
            ));
        }

        // GET: api/meal-histories/stats?date=2025-09-12
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetDailyStats([FromQuery] DateOnly date)
        {
            var userId = GetUserIdFromToken();
            var stats = await _service.GetDailyStatsAsync(userId, date);

            return Ok(ApiResponse<object>.SuccessResponse(
                stats,
                "Fetched daily nutrition stats successfully"
            ));
        }
    }
}
