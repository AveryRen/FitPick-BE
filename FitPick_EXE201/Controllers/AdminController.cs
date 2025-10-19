using FitPick_EXE201.Helpers;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitPick_EXE201.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [AllowAnonymous] // Allow anonymous access for seeding, remove in production
    public class AdminController : ControllerBase
    {
        private readonly IAdminDataService _adminDataService;
        private readonly UserService _userService;
        private readonly FitPickContext _context;

        public AdminController(IAdminDataService adminDataService, UserService userService, FitPickContext context)
        {
            _adminDataService = adminDataService;
            _userService = userService;
            _context = context;
        }

        [HttpPost("seed-data")]
        public async Task<ActionResult<ApiResponse<string>>> SeedData()
        {
            try
            {
                var result = await _adminDataService.SeedDataAsync();
                
                if (result)
                {
                    return Ok(ApiResponse<string>.SuccessResponse("Data seeded successfully", "Data seeded successfully"));
                }
                else
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse(new List<string> { "Failed to seed data" }, "Failed to seed data"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(new List<string> { ex.Message }, "Failed to seed data"));
            }
        }

        [HttpGet("check-data")]
        public async Task<ActionResult<ApiResponse<object>>> CheckData()
        {
            try
            {
                var data = await _adminDataService.CheckDataAsync();
                return Ok(ApiResponse<object>.SuccessResponse(data, "Data retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, "Failed to retrieve data"));
            }
        }

        [HttpGet("debug-user-data")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DebugUserData()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { "User ID not found" }, "User ID not found"));
                }

                // Get user info
                var user = await _context.Users
                    .Include(u => u.DietPlan)
                    .Include(u => u.CookingLevel)
                    .FirstOrDefaultAsync(u => u.Userid == userId);

                // Get healthprofile
                var healthProfile = await _context.Healthprofiles
                    .Include(hp => hp.Healthgoal)
                    .Include(hp => hp.Lifestyle)
                    .FirstOrDefaultAsync(hp => hp.Userid == userId);

                // Get all available data
                var allHealthGoals = await _context.Healthgoals.ToListAsync();
                var allLifestyles = await _context.Lifestyles.ToListAsync();
                var allDietPlans = await _context.DietPlans.ToListAsync();
                var allCookingLevels = await _context.CookingLevels.ToListAsync();

                var result = new
                {
                    userId = userId,
                    user = user != null ? new
                    {
                        user.Userid,
                        user.Fullname,
                        user.Email,
                        user.Age,
                        user.Height,
                        user.Weight,
                        user.TargetWeight,
                        user.GenderId,
                        user.DietPlanId,
                        user.CookingLevelId,
                        user.IsOnboardingCompleted,
                        user.OnboardingCompletedAt,
                        dietPlanName = user.DietPlan?.Name,
                        cookingLevelName = user.CookingLevel?.Name
                    } : null,
                    healthProfile = healthProfile != null ? new
                    {
                        healthProfile.Profileid,
                        healthProfile.Userid,
                        healthProfile.Healthgoalid,
                        healthProfile.Lifestyleid,
                        healthProfile.Updatedat,
                        healthGoalName = healthProfile.Healthgoal?.Name,
                        lifestyleName = healthProfile.Lifestyle?.Name
                    } : null,
                    availableData = new
                    {
                        healthGoals = allHealthGoals.Select(hg => new { hg.Id, hg.Name }),
                        lifestyles = allLifestyles.Select(l => new { l.Id, l.Name }),
                        dietPlans = allDietPlans.Select(dp => new { dp.Id, dp.Name }),
                        cookingLevels = allCookingLevels.Select(cl => new { cl.Id, cl.Name })
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(result, "User debug data retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, "Failed to retrieve user debug data"));
            }
        }
    }
}
