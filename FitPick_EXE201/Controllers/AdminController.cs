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
    [Authorize(Roles = "Admin")]
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
        [AllowAnonymous] // Keep this for seeding
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

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> GetDashboardStats()
        {
            try
            {
                // Get total users count
                var totalUsers = await _context.Users.CountAsync(u => u.RoleId != 4); // Exclude admin users

                // Get total revenue from completed payments
                var totalRevenue = await _context.PayosPayments
                    .Where(p => p.Status != null && p.Status.ToUpper() == "PAID")
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                // Get total orders/transactions count
                var totalOrders = await _context.PayosPayments.CountAsync();

                // Get total meals/products count
                var totalProducts = await _context.Meals.CountAsync();

                var stats = new
                {
                    totalUsers,
                    revenue = totalRevenue,
                    orders = totalOrders,
                    products = totalProducts
                };

                return Ok(ApiResponse<object>.SuccessResponse(stats, "Dashboard stats retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, "Failed to retrieve dashboard stats"));
            }
        }

        [HttpGet("analytics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> GetAnalytics(
            [FromQuery] string? userPeriod = "month", // "month", "quarter", "year"
            [FromQuery] string? dietPeriod = "month", // "month", "quarter", "year"
            [FromQuery] string? mealPeriod = "week" // "week", "month", "quarter"
        )
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                var lastWeekStart = thisWeekStart.AddDays(-7);
                var lastWeekEnd = thisWeekStart.AddDays(-1);
                var thisMonthStart = new DateOnly(today.Year, today.Month, 1);
                var lastMonthStart = thisMonthStart.AddMonths(-1);
                var lastMonthEnd = thisMonthStart.AddDays(-1);
                
                // Calculate date ranges based on periods
                DateOnly userPeriodStart, userPeriodEnd;
                DateOnly dietPeriodStart, dietPeriodEnd;
                DateOnly mealPeriodStart, mealPeriodEnd;
                
                // User demographics period
                switch (userPeriod?.ToLower())
                {
                    case "quarter":
                        var currentQuarter = (today.Month - 1) / 3;
                        userPeriodStart = new DateOnly(today.Year, currentQuarter * 3 + 1, 1);
                        userPeriodEnd = today;
                        break;
                    case "year":
                        userPeriodStart = new DateOnly(today.Year, 1, 1);
                        userPeriodEnd = today;
                        break;
                    default: // "month"
                        userPeriodStart = thisMonthStart;
                        userPeriodEnd = today;
                        break;
                }
                
                // Diet period
                switch (dietPeriod?.ToLower())
                {
                    case "quarter":
                        var currentQuarterDiet = (today.Month - 1) / 3;
                        dietPeriodStart = new DateOnly(today.Year, currentQuarterDiet * 3 + 1, 1);
                        dietPeriodEnd = today;
                        break;
                    case "year":
                        dietPeriodStart = new DateOnly(today.Year, 1, 1);
                        dietPeriodEnd = today;
                        break;
                    default: // "month"
                        dietPeriodStart = thisMonthStart;
                        dietPeriodEnd = today;
                        break;
                }
                
                // Meal period
                switch (mealPeriod?.ToLower())
                {
                    case "month":
                        mealPeriodStart = thisMonthStart;
                        mealPeriodEnd = today;
                        break;
                    case "quarter":
                        var currentQuarterMeal = (today.Month - 1) / 3;
                        mealPeriodStart = new DateOnly(today.Year, currentQuarterMeal * 3 + 1, 1);
                        mealPeriodEnd = today;
                        break;
                    default: // "week"
                        mealPeriodStart = thisWeekStart;
                        mealPeriodEnd = today;
                        break;
                }

                // Total Users
                var totalUsers = await _context.Users.CountAsync(u => u.RoleId != 4);

                // DAU - Daily Active Users (users with meal history today)
                var dau = await _context.MealHistories
                    .Where(mh => mh.Date == today)
                    .Select(mh => mh.Userid)
                    .Distinct()
                    .CountAsync();

                // MAU - Monthly Active Users (users with meal history this month)
                var mau = await _context.MealHistories
                    .Where(mh => mh.Date >= thisMonthStart && mh.Date <= today)
                    .Select(mh => mh.Userid)
                    .Distinct()
                    .CountAsync();

                // Weekly Growth - compare this week vs last week
                var thisWeekUsers = await _context.MealHistories
                    .Where(mh => mh.Date >= thisWeekStart && mh.Date <= today)
                    .Select(mh => mh.Userid)
                    .Distinct()
                    .CountAsync();

                var lastWeekUsers = await _context.MealHistories
                    .Where(mh => mh.Date >= lastWeekStart && mh.Date <= lastWeekEnd)
                    .Select(mh => mh.Userid)
                    .Distinct()
                    .CountAsync();

                var weeklyGrowth = lastWeekUsers > 0 
                    ? Math.Round(((double)(thisWeekUsers - lastWeekUsers) / lastWeekUsers) * 100, 1)
                    : 0.0;

                // Gender Distribution (filter by users created in period)
                var genderStats = await _context.Users
                    .Where(u => u.RoleId != 4 && u.GenderId != null &&
                                u.Createdat != null &&
                                DateOnly.FromDateTime(u.Createdat.Value) >= userPeriodStart &&
                                DateOnly.FromDateTime(u.Createdat.Value) <= userPeriodEnd)
                    .GroupBy(u => u.GenderId)
                    .Select(g => new
                    {
                        genderId = g.Key,
                        count = g.Count()
                    })
                    .ToListAsync();

                var totalWithGender = genderStats.Sum(g => g.count);
                var genderData = new
                {
                    male = genderStats.FirstOrDefault(g => g.genderId == 1)?.count ?? 0,
                    female = genderStats.FirstOrDefault(g => g.genderId == 2)?.count ?? 0,
                    other = genderStats.Where(g => g.genderId != 1 && g.genderId != 2).Sum(g => g.count),
                    malePercentage = totalWithGender > 0 ? (int)Math.Round((double)(genderStats.FirstOrDefault(g => g.genderId == 1)?.count ?? 0) / totalWithGender * 100) : 0,
                    femalePercentage = totalWithGender > 0 ? (int)Math.Round((double)(genderStats.FirstOrDefault(g => g.genderId == 2)?.count ?? 0) / totalWithGender * 100) : 0,
                    otherPercentage = totalWithGender > 0 ? (int)Math.Round((double)(genderStats.Where(g => g.genderId != 1 && g.genderId != 2).Sum(g => g.count)) / totalWithGender * 100) : 0
                };

                // Age Groups Distribution (filter by users created in period)
                var usersWithAge = await _context.Users
                    .Where(u => u.RoleId != 4 && u.Age != null &&
                                u.Createdat != null &&
                                DateOnly.FromDateTime(u.Createdat.Value) >= userPeriodStart &&
                                DateOnly.FromDateTime(u.Createdat.Value) <= userPeriodEnd)
                    .Select(u => u.Age ?? 0)
                    .ToListAsync();

                var genZ = usersWithAge.Count(a => a >= 18 && a <= 25);
                var millennials = usersWithAge.Count(a => a >= 26 && a <= 35);
                var genX = usersWithAge.Count(a => a >= 36 && a <= 45);
                var boomers = usersWithAge.Count(a => a > 45);

                var totalWithAge = usersWithAge.Count;
                var ageGroups = new[]
                {
                    new { name = "Gen Z (18-25)", count = genZ, percentage = totalWithAge > 0 ? (int)Math.Round((double)genZ / totalWithAge * 100) : 0, description = "Thế hệ số hóa, yêu thích công nghệ" },
                    new { name = "Millennials (26-35)", count = millennials, percentage = totalWithAge > 0 ? (int)Math.Round((double)millennials / totalWithAge * 100) : 0, description = "Thế hệ Y, quan tâm sức khỏe" },
                    new { name = "Gen X (36-45)", count = genX, percentage = totalWithAge > 0 ? (int)Math.Round((double)genX / totalWithAge * 100) : 0, description = "Thế hệ X, có thu nhập ổn định" },
                    new { name = "Boomers (>45)", count = boomers, percentage = totalWithAge > 0 ? (int)Math.Round((double)boomers / totalWithAge * 100) : 0, description = "Thế hệ trưởng thành, quan tâm dinh dưỡng" }
                };

                // Diet Plan Distribution (filter by users created in period)
                var dietStats = await _context.Users
                    .Where(u => u.RoleId != 4 && u.DietPlanId != null && 
                                u.Createdat != null &&
                                DateOnly.FromDateTime(u.Createdat.Value) >= dietPeriodStart &&
                                DateOnly.FromDateTime(u.Createdat.Value) <= dietPeriodEnd)
                    .Include(u => u.DietPlan)
                    .GroupBy(u => new { u.DietPlanId, u.DietPlan!.Name })
                    .Select(g => new
                    {
                        name = g.Key.Name,
                        count = g.Count()
                    })
                    .ToListAsync();

                var totalWithDiet = dietStats.Sum(d => d.count);
                var dietData = dietStats.Select(d => new
                {
                    name = d.name,
                    value = d.count,
                    percentage = totalWithDiet > 0 ? (int)Math.Round((double)d.count / totalWithDiet * 100) : 0
                }).OrderByDescending(d => d.value).ToList();

                // Top Popular Meals (filtered by period)
                var popularMeals = await _context.MealHistories
                    .Where(mh => mh.Date >= mealPeriodStart && mh.Date <= mealPeriodEnd && mh.Meal != null)
                    .GroupBy(mh => new { mh.Mealid, mh.Meal!.Name })
                    .Select(g => new
                    {
                        mealId = g.Key.Mealid,
                        name = g.Key.Name,
                        orders = g.Count()
                    })
                    .OrderByDescending(m => m.orders)
                    .Take(8)
                    .ToListAsync();

                var totalMealOrders = popularMeals.Sum(m => m.orders);
                var popularMealsWithPercentage = popularMeals.Select((m, index) => new
                {
                    rank = index + 1,
                    name = m.name,
                    orders = m.orders,
                    percentage = totalMealOrders > 0 ? Math.Round((double)m.orders / totalMealOrders * 100, 1) : 0,
                    trend = "up" // Could be calculated by comparing with last week
                }).ToList();

                var result = new
                {
                    kpi = new
                    {
                        totalUsers,
                        dau,
                        mau,
                        weeklyGrowth
                    },
                    gender = genderData,
                    ageGroups = ageGroups,
                    dietPlans = dietData,
                    popularMeals = popularMealsWithPercentage,
                    totalMealOrders = totalMealOrders,
                    periods = new
                    {
                        userPeriod = userPeriod,
                        dietPeriod = dietPeriod,
                        mealPeriod = mealPeriod
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(result, "Analytics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, "Failed to retrieve analytics"));
            }
        }
    }
}