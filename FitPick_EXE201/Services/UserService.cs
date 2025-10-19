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

                // Find or create health profile
                var healthProfile = await _context.Healthprofiles
                    .FirstOrDefaultAsync(hp => hp.Userid == userId);


                if (healthProfile == null)
                {
                    healthProfile = new Healthprofile
                    {
                        Userid = userId,
                        Status = true,
                        Updatedat = DateTime.Now
                    };
                    _context.Healthprofiles.Add(healthProfile);
                }

                // Get all health goals for debugging
                var allHealthGoals = await _context.Healthgoals.ToListAsync();

                // Map frontend goal key to backend HealthGoalId using database lookup
                var healthGoalId = await MapGoalKeyToHealthGoalIdAsync(request.Goal);


                if (healthGoalId.HasValue)
                {
                    healthProfile.Healthgoalid = healthGoalId.Value;
                }
                else
                {
                    return false;
                }

                // Also save target weight to User table if provided
                if (!string.IsNullOrEmpty(request.OtherGoal) && decimal.TryParse(request.OtherGoal, out decimal targetWeight))
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.TargetWeight = targetWeight;
                        _context.Users.Update(user);
                    }
                }

                healthProfile.Updatedat = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<int?> MapGoalKeyToHealthGoalIdAsync(string goalKey)
        {
            // Ensure data is seeded first
            await EnsureDataSeededAsync();

            // First try exact matches with English keys
            var healthGoal = await _context.Healthgoals
                .FirstOrDefaultAsync(hg => hg.Name.ToLower() == goalKey.ToLower());

            if (healthGoal != null)
            {
                return healthGoal.Id;
            }

            // Then try mapping based on common patterns
            var mappedName = goalKey.ToLower() switch
            {
                "healthy" => "An u?ng l�nh m?nh",
                "lose" => "Gi?m c�n",
                "gain" => "Tang co",
                "gain weight" => "Tang c�n",
                "target" => "Duy tr� c�n n?ng",
                "other" => "An u?ng l�nh m?nh",
                _ => null
            };

            if (mappedName != null)
            {
                healthGoal = await _context.Healthgoals
                    .FirstOrDefaultAsync(hg => hg.Name == mappedName);

                if (healthGoal != null)
                {
                    return healthGoal.Id;
                }
            }

            // Finally try partial matches
            healthGoal = await _context.Healthgoals
                .FirstOrDefaultAsync(hg => hg.Name.ToLower().Contains(goalKey.ToLower()) ||
                                         goalKey.ToLower().Contains(hg.Name.ToLower()));

            if (healthGoal != null)
            {
                return healthGoal.Id;
            }

            return null;
        }

        public async Task<bool> SaveUserLifestyleAsync(int userId, SaveUserLifestyleRequest request)
        {
            try
            {

                // Find or create health profile
                var healthProfile = await _context.Healthprofiles
                    .FirstOrDefaultAsync(hp => hp.Userid == userId);


                if (healthProfile == null)
                {
                    healthProfile = new Healthprofile
                    {
                        Userid = userId,
                        Status = true,
                        Updatedat = DateTime.Now
                    };
                    _context.Healthprofiles.Add(healthProfile);
                }

                // Get all lifestyles for debugging
                var allLifestyles = await _context.Lifestyles.ToListAsync();

                // Map frontend activity level key to backend LifestyleId using database lookup
                var lifestyleId = await MapActivityLevelToLifestyleIdAsync(request.ActivityLevel);


                if (lifestyleId.HasValue)
                {
                    healthProfile.Lifestyleid = lifestyleId.Value;
                }
                else
                {
                    return false;
                }

                healthProfile.Updatedat = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<int?> MapActivityLevelToLifestyleIdAsync(string activityLevel)
        {
            // Ensure data is seeded first
            await EnsureDataSeededAsync();

            // First try exact matches with English keys
            var lifestyle = await _context.Lifestyles
                .FirstOrDefaultAsync(l => l.Name.ToLower() == activityLevel.ToLower());

            if (lifestyle != null)
            {
                return lifestyle.Id;
            }

            // Then try mapping based on common patterns
            var mappedName = activityLevel.ToLower() switch
            {
                "sedentary" => "�t v?n d?ng",
                "light" => "V?n d?ng nh?",
                "moderate" => "V?n d?ng v?a ph?i",
                "high" => "V?n d?ng nhi?u",
                _ => null
            };

            if (mappedName != null)
            {
                lifestyle = await _context.Lifestyles
                    .FirstOrDefaultAsync(l => l.Name == mappedName);

                if (lifestyle != null)
                {
                    return lifestyle.Id;
                }
            }

            // Finally try partial matches
            lifestyle = await _context.Lifestyles
                .FirstOrDefaultAsync(l => l.Name.ToLower().Contains(activityLevel.ToLower()) ||
                                         activityLevel.ToLower().Contains(l.Name.ToLower()));

            if (lifestyle != null)
            {
                return lifestyle.Id;
            }

            return null;
        }

        public async Task<bool> SaveUserDietPlanAsync(int userId, SaveUserDietPlanRequest request)
        {
            try
            {

                // Ensure data is seeded first
                await EnsureDataSeededAsync();

                var user = await _context.Users.FindAsync(userId);
                if (user == null) 
                {
                    return false;
                }

                // Get all diet plans for debugging
                var allDietPlans = await _context.DietPlans.ToListAsync();

                // Map diet plan string to DietPlanId
                var dietPlan = await _context.DietPlans.FirstOrDefaultAsync(dp => dp.Name.ToLower() == request.DietPlan.ToLower());
                
                if (dietPlan != null)
                {
                    user.DietPlanId = dietPlan.Id;
                }
                else
                {
                    return false;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
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

                if (user == null)
                {
                    return null;
                }


                // Get health profile information (goals and activity level)
                var healthProfile = await _context.Healthprofiles
                    .Include(hp => hp.Healthgoal)
                    .Include(hp => hp.Lifestyle)
                    .FirstOrDefaultAsync(hp => hp.Userid == userId);

                if (healthProfile != null)
                {
                }

            var result = new UserProfileDto
            {
                FullName = user.Fullname ?? "",
                Email = user.Email ?? "",
                Age = user.Age ?? 0,
                Height = (int)(user.Height ?? 0),
                Weight = (int)(user.Weight ?? 0),
                TargetWeight = (int)(user.TargetWeight ?? 0),
                Gender = user.GenderId == 1 ? "Nam" : "N?",
                DietPlan = user.DietPlan?.Name ?? "",
                CookingLevel = user.CookingLevel?.Name ?? "",
                Goal = healthProfile?.Healthgoal?.Name ?? "",
                OtherGoal = user.TargetWeight?.ToString(), // Use TargetWeight as OtherGoal
                ActivityLevel = healthProfile?.Lifestyle?.Name ?? "",
                IsOnboardingCompleted = user.IsOnboardingCompleted ?? false,
                AvatarUrl = user.AvatarUrl ?? "https://i.pravatar.cc/100?img=1",
                AccountType = "FREE", // C� th? th�m logic d? check Premium sau
                Country = user.Country ?? "",
                TargetCalories = CalculateTargetCalories(user)
            };

                return result;
            }
            catch (Exception ex)
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

            // Default activity multiplier (c� th? l?y t? healthprofile sau)
            var activityMultiplier = 1.375; // V?a ph?i

            return (int)(bmr * activityMultiplier);
        }

        private async Task EnsureDataSeededAsync()
        {
            try
            {
                // Always check and add missing English keys
                var existingGoals = await _context.Healthgoals.ToListAsync();
                var existingNames = existingGoals.Select(hg => hg.Name.ToLower()).ToHashSet();
                
                var missingGoals = new List<Healthgoal>();
                
                // Add missing English keys
                if (!existingNames.Contains("healthy"))
                    missingGoals.Add(new Healthgoal { Name = "healthy", CalorieAdjustment = 0 });
                if (!existingNames.Contains("lose"))
                    missingGoals.Add(new Healthgoal { Name = "lose", CalorieAdjustment = -500 });
                if (!existingNames.Contains("gain"))
                    missingGoals.Add(new Healthgoal { Name = "gain", CalorieAdjustment = 300 });
                if (!existingNames.Contains("gain weight"))
                    missingGoals.Add(new Healthgoal { Name = "gain weight", CalorieAdjustment = 500 });
                if (!existingNames.Contains("target"))
                    missingGoals.Add(new Healthgoal { Name = "target", CalorieAdjustment = 0 });
                if (!existingNames.Contains("lose weight"))
                    missingGoals.Add(new Healthgoal { Name = "lose weight", CalorieAdjustment = -500 });
                if (!existingNames.Contains("gain muscle"))
                    missingGoals.Add(new Healthgoal { Name = "gain muscle", CalorieAdjustment = 300 });
                
                // Add missing Vietnamese names
                if (!existingNames.Contains("gi?m c�n"))
                    missingGoals.Add(new Healthgoal { Name = "Gi?m c�n", CalorieAdjustment = -500 });
                if (!existingNames.Contains("tang c�n"))
                    missingGoals.Add(new Healthgoal { Name = "Tang c�n", CalorieAdjustment = 500 });
                if (!existingNames.Contains("duy tr� c�n n?ng"))
                    missingGoals.Add(new Healthgoal { Name = "Duy tr� c�n n?ng", CalorieAdjustment = 0 });
                if (!existingNames.Contains("tang co"))
                    missingGoals.Add(new Healthgoal { Name = "Tang co", CalorieAdjustment = 300 });
                if (!existingNames.Contains("an u?ng l�nh m?nh"))
                    missingGoals.Add(new Healthgoal { Name = "An u?ng l�nh m?nh", CalorieAdjustment = 0 });
                
                if (missingGoals.Any())
                {
                    _context.Healthgoals.AddRange(missingGoals);
                    await _context.SaveChangesAsync();
                }

                // Check if lifestyles exist, if not seed them
                if (!await _context.Lifestyles.AnyAsync())
                {
                    var lifestyles = new List<Lifestyle>
                    {
                        new Lifestyle { Name = "�t v?n d?ng", Multiplier = 1.2m },
                        new Lifestyle { Name = "V?n d?ng nh?", Multiplier = 1.375m },
                        new Lifestyle { Name = "V?n d?ng v?a ph?i", Multiplier = 1.55m },
                        new Lifestyle { Name = "V?n d?ng nhi?u", Multiplier = 1.725m },
                        new Lifestyle { Name = "V?n d?ng r?t nhi?u", Multiplier = 1.9m },
                        new Lifestyle { Name = "sedentary", Multiplier = 1.2m },
                        new Lifestyle { Name = "lightly active", Multiplier = 1.375m },
                        new Lifestyle { Name = "moderately active", Multiplier = 1.55m },
                        new Lifestyle { Name = "very active", Multiplier = 1.725m },
                        new Lifestyle { Name = "extra active", Multiplier = 1.9m }
                    };
                    _context.Lifestyles.AddRange(lifestyles);
                    await _context.SaveChangesAsync();
                }

                // Check and add missing diet plans
                var existingDietPlans = await _context.DietPlans.ToListAsync();
                var existingDietPlanNames = existingDietPlans.Select(dp => dp.Name.ToLower()).ToHashSet();
                
                var missingDietPlans = new List<DietPlan>();
                
                if (!existingDietPlanNames.Contains("gluten free"))
                    missingDietPlans.Add(new DietPlan { Name = "Gluten Free", Description = "Tr�nh ho�n to�n th?c ph?m ch?a gluten", Status = true, CreatedAt = DateTime.Now });
                if (!existingDietPlanNames.Contains("dairy free"))
                    missingDietPlans.Add(new DietPlan { Name = "Dairy Free", Description = "Tr�nh ho�n to�n th?c ph?m t? s?a", Status = true, CreatedAt = DateTime.Now });
                
                if (missingDietPlans.Any())
                {
                    _context.DietPlans.AddRange(missingDietPlans);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<bool> DeactivateAccountAsync(int userId)
        {
            try
            {
                // Find the user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Deactivate account: Set status to false to prevent login
                user.Status = false;
                user.Updatedat = DateTime.Now;
                
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
