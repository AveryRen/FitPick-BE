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
            return int.Parse(User.FindFirst("id")?.Value ??
                             throw new UnauthorizedAccessException("User ID not found in token"));
        }


        // GET: api/meal-histories
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<MealHistoryDto>>>> GetAllByUser()
        {
            var userId = GetUserIdFromToken();
            var histories = await _service.GetUserHistoryAsync(userId);

            // Map sang DTO, bao gồm các object liên quan
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
                    h.Meal.Calories
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

            // Tạo entity từ DTO request
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

            // (tùy bạn có muốn check ownership không)
            await _service.DeleteMealHistoryAsync(id);

            return Ok(ApiResponse<string>.SuccessResponse(
                "Deleted",
                "Meal history deleted successfully"
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