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
                Console.WriteLine($"=== GetMealDetail called for mealId: {mealId} ===");
                
                var mealDetail = await _mealDetailService.GetMealDetailByIdAsync(mealId);
                
                if (mealDetail == null)
                {
                    Console.WriteLine($"Meal {mealId} not found");
                    return NotFound(ApiResponse<MealDetailDto>.ErrorResponse(
                        new List<string> { "Meal not found" }, 
                        "Meal not found"));
                }

                // Log instructions count for debugging
                var instructionsCount = mealDetail.Instructions?.Count ?? 0;
                Console.WriteLine($"MealDetail API: Meal {mealId} has {instructionsCount} instructions");
                
                if (instructionsCount > 0)
                {
                    Console.WriteLine("Instructions details:");
                    foreach (var inst in mealDetail.Instructions!)
                    {
                        Console.WriteLine($"  - Step {inst.StepNumber}: {inst.Instruction}");
                    }
                }
                else
                {
                    Console.WriteLine($"WARNING: No instructions found for meal {mealId}");
                }

                // Log full DTO for debugging
                Console.WriteLine($"MealDetail DTO - Name: {mealDetail.Name}, Instructions: {(mealDetail.Instructions != null ? mealDetail.Instructions.Count : 0)}");

                return Ok(ApiResponse<MealDetailDto>.SuccessResponse(
                    mealDetail, 
                    "Meal detail retrieved successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetMealDetail: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, ApiResponse<MealDetailDto>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Failed to retrieve meal detail"));
            }
        }
    }
}
