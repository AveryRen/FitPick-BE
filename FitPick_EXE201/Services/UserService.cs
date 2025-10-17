using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Repositories.Interface;
using FitPick_EXE201.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class UserService
    {
        private readonly IUserRepo _userRepo;
        private readonly FitPickContext _context;

        public UserService(IUserRepo userRepo, FitPickContext context)
        {
            _userRepo = userRepo;
            _context = context;
        }

        public async Task<UserProfileDto?> GetUserByIdAsync(int id)
        {
            return await _userRepo.GetUserByIdAsync(id);
        }

        public async Task<UpdateUserProfileDto?> UpdateProfileAsync(int userId, UpdateUserProfileRequest request)
        {
            return await _userRepo.UpdateProfileAsync(userId, request);
        } 
        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
             string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            return await _userRepo.ChangePasswordAsync(userId, hashedPassword);
        }

        public async Task<bool> ChangeAvatarAsync(int userId, string avatarUrl)
        {
            return await _userRepo.ChangeAvatarAsync(userId, avatarUrl);
        }

        // Onboarding methods
        public async Task<bool> SaveUserProfileAsync(int userId, SaveUserProfileRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.Fullname = request.FullName;
                user.Age = request.Age;
                user.Height = request.Height;
                user.Weight = request.Weight;
                user.TargetWeight = request.TargetWeight;
                
                // Map gender string to GenderId
                user.GenderId = request.Gender.ToLower() switch
                {
                    "male" => 1,
                    "female" => 2,
                    _ => 1
                };

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveUserGoalsAsync(int userId, SaveUserGoalsRequest request)
        {
            try
            {
                // For now, we'll store this in a simple way
                // You might want to create a separate UserGoals table
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Store goal in a JSON field or create a separate table
                // For demo purposes, we'll just mark that goals are saved
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveUserLifestyleAsync(int userId, SaveUserLifestyleRequest request)
        {
            try
            {
                // Similar to goals, store lifestyle information
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveUserDietPlanAsync(int userId, SaveUserDietPlanRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Map diet plan string to DietPlanId
                var dietPlan = await _context.DietPlans.FirstOrDefaultAsync(dp => dp.Name.ToLower() == request.DietPlan.ToLower());
                if (dietPlan != null)
                {
                    user.DietPlanId = dietPlan.Id;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveUserCookingLevelAsync(int userId, SaveUserCookingLevelRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Map cooking level string to CookingLevelId
                var cookingLevel = await _context.CookingLevels.FirstOrDefaultAsync(cl => cl.Name.ToLower() == request.CookingLevel.ToLower());
                if (cookingLevel != null)
                {
                    user.CookingLevelId = cookingLevel.Id;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CompleteOnboardingAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Mark onboarding as completed
                user.IsOnboardingCompleted = true;
                user.OnboardingCompletedAt = DateTime.Now;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.DietPlan)
                    .Include(u => u.CookingLevel)
                    .FirstOrDefaultAsync(u => u.Userid == userId);
                
                if (user == null) return null;

                return new UserProfileDto
                {
                    FullName = user.Fullname ?? "",
                    Email = user.Email ?? "",
                    Age = user.Age ?? 0,
                    Height = (int)(user.Height ?? 0),
                    Weight = (int)(user.Weight ?? 0),
                    TargetWeight = (int)(user.TargetWeight ?? 0),
                    Gender = user.GenderId == 1 ? "Nam" : "Nữ",
                    DietPlan = user.DietPlan?.Name ?? "",
                    CookingLevel = user.CookingLevel?.Name ?? "",
                    IsOnboardingCompleted = user.IsOnboardingCompleted ?? false,
                    AvatarUrl = user.AvatarUrl ?? "https://i.pravatar.cc/100?img=1",
                    AccountType = "FREE", // Có thể thêm logic để check Premium sau
                    TargetCalories = CalculateTargetCalories(user)
                };
            }
            catch
            {
                return null;
            }
        }

        // Helper method to calculate target calories
        private int? CalculateTargetCalories(User user)
        {
            if (user.Age == null || user.Height == null || user.Weight == null)
                return null;

            var weight = (double)user.Weight;
            var height = (double)user.Height;
            var age = (int)user.Age;
            var isMale = user.GenderId == 1;

            // Calculate BMR (Basal Metabolic Rate) using Mifflin-St Jeor Equation
            double bmr;
            if (isMale)
            {
                bmr = 10 * weight + 6.25 * height - 5 * age + 5;
            }
            else
            {
                bmr = 10 * weight + 6.25 * height - 5 * age - 161;
            }

            // Default activity multiplier (có thể lấy từ healthprofile sau)
            var activityMultiplier = 1.375; // Vừa phải

            return (int)(bmr * activityMultiplier);
        }
    }
}