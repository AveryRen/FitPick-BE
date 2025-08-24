using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Premium,Admin")]
    public class SpendinglogController : ControllerBase
    {
        private readonly SpendinglogService _service;

        public SpendinglogController(SpendinglogService service)
        {
            _service = service;
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }

        // POST api/spendinglog/add
        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<SpendinglogDTO>>> AddSpending(decimal amount, string? note = null)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<SpendinglogDTO>.ErrorResponse(
                    new List<string> { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));
            }

            var result = await _service.AddSpendingAsync(userId.Value, amount, note);
            return Ok(ApiResponse<SpendinglogDTO>.SuccessResponse(result, "Spending log added successfully"));
        }

        // POST api/spendinglog/add-meal
        [HttpPost("add-meal")]
        public async Task<ActionResult<ApiResponse<SpendinglogDTO>>> AddSpendingForMeal([FromBody] AddMealSpendingRequest request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<SpendinglogDTO>.ErrorResponse(
                    new() { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));

            var result = await _service.AddSpendingForMealAsync(userId.Value, request.MealId);
            return Ok(ApiResponse<SpendinglogDTO>.SuccessResponse(result, "Spending log for meal added successfully"));
        }

        // POST api/spendinglog/add-meals
        [HttpPost("add-meals")]
        public async Task<ActionResult<ApiResponse<List<SpendinglogDTO>>>> AddSpendingForMeals([FromBody] AddMealsSpendingRequest request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(ApiResponse<List<SpendinglogDTO>>.ErrorResponse(
                    new() { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));

            var result = await _service.AddSpendingForMealsAsync(userId.Value, request.MealIds);
            return Ok(ApiResponse<List<SpendinglogDTO>>.SuccessResponse(result, "Spending logs for meals added successfully"));
        }

        // GET api/spendinglog/daily-total?date=2025-08-24
        [HttpGet("daily-total")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetDailyTotal(DateOnly date)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<decimal>.ErrorResponse(
                    new List<string> { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));
            }

            var total = await _service.GetDailyTotalAsync(userId.Value, date);
            return Ok(ApiResponse<decimal>.SuccessResponse(total, "Get daily total successfully"));
        }

        // GET api/spendinglog/monthly-total?year=2025&month=8
        [HttpGet("monthly-total")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetMonthlyTotal(int year, int month)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<decimal>.ErrorResponse(
                    new List<string> { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));
            }

            var total = await _service.GetMonthlyTotalAsync(userId.Value, year, month);
            return Ok(ApiResponse<decimal>.SuccessResponse(total, "Get monthly total successfully"));
        }

        // GET api/spendinglog/yearly-total?year=2025
        [HttpGet("yearly-total")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetYearlyTotal(int year)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<decimal>.ErrorResponse(
                    new List<string> { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));
            }

            var total = await _service.GetYearlyTotalAsync(userId.Value, year);
            return Ok(ApiResponse<decimal>.SuccessResponse(total, "Get yearly total successfully"));
        }

        // GET api/spendinglog/range?start=2025-08-01&end=2025-08-24
        [HttpGet("range")]
        public async Task<ActionResult<ApiResponse<List<SpendinglogDTO>>>> GetSpendingInRange(DateOnly start, DateOnly end)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<List<SpendinglogDTO>>.ErrorResponse(
                    new List<string> { "Invalid or missing user ID in token." },
                    "Invalid or missing user ID in token."
                ));
            }

            var logs = await _service.GetSpendingInRangeAsync(userId.Value, start, end);
            return Ok(ApiResponse<List<SpendinglogDTO>>.SuccessResponse(logs, "Get spending logs in range successfully"));
        }
    }
}
