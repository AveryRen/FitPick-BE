using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FitPick_EXE201.Services;
using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.DTOs;

namespace FitPick_EXE201.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MealDetailController : ControllerBase
    {
        private readonly IMealDetailService _mealDetailService;

        public MealDetailController(IMealDetailService mealDetailService)
        {
            _mealDetailService = mealDetailService;
        }

        [HttpGet("{mealId}")]
        public async Task<ActionResult<ApiResponse<MealDetailDto>>> GetMealDetail(int mealId)
        {
            try
            {
                var mealDetail = await _mealDetailService.GetMealDetailByIdAsync(mealId);
                
                if (mealDetail == null)
                {
                    return NotFound(ApiResponse<MealDetailDto>.ErrorResponse(
                        new List<string> { "Meal not found" }, 
                        "Meal not found"));
                }

                return Ok(ApiResponse<MealDetailDto>.SuccessResponse(
                    mealDetail, 
                    "Meal detail retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MealDetailDto>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Failed to retrieve meal detail"));
            }
        }
    }
}
