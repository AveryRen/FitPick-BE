using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitPick_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MealPlansController : ControllerBase
    {
        private readonly MealPlanService _mealPlanService;

        public MealPlansController(MealPlanService mealPlanService)
        {
            _mealPlanService = mealPlanService;
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodayMealPlan()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); 
            if (userIdClaim == null)
                return Unauthorized("UserId not found in token");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Invalid userId in token");
            var today = DateTime.Now;
            var plans = await _mealPlanService.GetTodayMealPlanAsync(userId, today);
            return Ok(plans);
        }
    }
}
