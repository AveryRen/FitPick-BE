using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;
using System.Threading.Tasks;

namespace FitPick_EXE201.Services
{
    public class ProUserService
    {
        private readonly IUserPremiumRepo _userPremiumRepo;
        private readonly IUserRepo _userRepo;

        public ProUserService(IUserPremiumRepo userPremiumRepo, IUserRepo userRepo)
        {
            _userPremiumRepo = userPremiumRepo;
            _userRepo = userRepo;
        }

        public async Task<bool> IsProUserAsync(int userId)
        {
            return await _userPremiumRepo.IsUserPremiumAsync(userId);
        }

        public async Task<ProUserInfo?> GetProUserInfoAsync(int userId)
        {
            var userProfile = await _userRepo.GetUserByIdAsync(userId);
            if (userProfile == null || userProfile.AccountType != "PRO")
            {
                return null;
            }

            return new ProUserInfo
            {
                UserId = userId,
                IsPro = true,
                IsProUser = true, // Match frontend interface
                CanViewFutureDates = true,
                CanPlanFutureMeals = true,
                CanViewPastDates = true, // Always true for both FREE and PRO
                CanUsePremiumFeatures = true, // Match frontend interface
                CanCreateWeeklyMealPlan = true, // Match frontend interface
                CanGenerateWeeklyMealPlan = true, // Keep for backward compatibility
                CanAccessAIFeatures = true, // Keep for backward compatibility
                CanViewDetailedReports = true // Match frontend interface
            };
        }

        public async Task<bool> CanViewFutureDatesAsync(int userId)
        {
            return await IsProUserAsync(userId);
        }

        public async Task<bool> CanPlanFutureMealsAsync(int userId)
        {
            return await IsProUserAsync(userId);
        }

        public async Task<bool> CanUsePremiumFeaturesAsync(int userId)
        {
            return await IsProUserAsync(userId);
        }

        public async Task<bool> CanCreateWeeklyMealPlanAsync(int userId)
        {
            return await IsProUserAsync(userId);
        }

        public async Task<bool> CanViewDetailedReportsAsync(int userId)
        {
            return await IsProUserAsync(userId);
        }
    }

    public class ProUserInfo
    {
        public int UserId { get; set; }
        public bool IsPro { get; set; }
        public bool IsProUser { get; set; } // Match frontend interface
        public bool CanViewFutureDates { get; set; }
        public bool CanPlanFutureMeals { get; set; }
        public bool CanViewPastDates { get; set; } // Always true for both FREE and PRO
        public bool CanUsePremiumFeatures { get; set; } // Match frontend interface
        public bool CanCreateWeeklyMealPlan { get; set; } // Match frontend interface
        public bool CanGenerateWeeklyMealPlan { get; set; } // Keep for backward compatibility
        public bool CanAccessAIFeatures { get; set; } // Keep for backward compatibility
        public bool CanViewDetailedReports { get; set; } // Match frontend interface
    }
}
