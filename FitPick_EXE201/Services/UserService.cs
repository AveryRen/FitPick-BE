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

        public async Task<bool> CreateHealthProfileAsync(int userId, CreateHealthProfileRequest request)
        {
            try
            {
                // Check if health profile already exists
                var existingProfile = await _context.Healthprofiles
                    .FirstOrDefaultAsync(hp => hp.Userid == userId);
                
                if (existingProfile != null)
                {
                    // Update existing profile
                    existingProfile.Healthgoalid = request.HealthGoalId;
                    existingProfile.Lifestyleid = request.LifestyleId;
                    existingProfile.Targetcalories = request.TargetCalories ?? 2000;
                    existingProfile.Targetweight = request.TargetWeight;
                    existingProfile.Dailymeals = request.DailyMeals ?? 3;
                    existingProfile.Status = true;
                    existingProfile.Updatedat = DateTime.Now;
                    
                    _context.Healthprofiles.Update(existingProfile);
                }
                else
                {
                    // Create new profile
                    var newProfile = new Healthprofile
                    {
                        Userid = userId,
                        Healthgoalid = request.HealthGoalId,
                        Lifestyleid = request.LifestyleId,
                        Targetcalories = request.TargetCalories ?? 2000,
                        Targetweight = request.TargetWeight,
                        Dailymeals = request.DailyMeals ?? 3,
                        Status = true,
                        Updatedat = DateTime.Now
                    };
                    
                    _context.Healthprofiles.Add(newProfile);
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating health profile: {ex.Message}");
                return false;
            }
        }

        public async Task<object> DebugUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.DietPlan)
                    .Include(u => u.CookingLevel)
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Userid == userId);

                if (user == null)
                {
                    return new { error = "User not found" };
                }

                var healthProfile = await _context.Healthprofiles
                    .Include(hp => hp.Healthgoal)
                    .Include(hp => hp.Lifestyle)
                    .FirstOrDefaultAsync(hp => hp.Userid == userId);

                // Check DietPlans table
                var dietPlans = await _context.DietPlans.ToListAsync();
                var cookingLevels = await _context.CookingLevels.ToListAsync();
                var healthGoals = await _context.Healthgoals.ToListAsync();
                var lifestyles = await _context.Lifestyles.ToListAsync();

                return new
                {
                    user = new
                    {
                        userId = user.Userid,
                        dietPlanId = user.DietPlanId,
                        cookingLevelId = user.CookingLevelId,
                        dietPlan = user.DietPlan?.Name,
                        cookingLevel = user.CookingLevel?.Name
                    },
                    healthProfile = healthProfile != null ? new
                    {
                        healthGoalId = healthProfile.Healthgoalid,
                        lifestyleId = healthProfile.Lifestyleid,
                        goal = healthProfile.Healthgoal?.Name,
                        activityLevel = healthProfile.Lifestyle?.Name
                    } : null,
                    availableData = new
                    {
                        dietPlans = dietPlans.Select(dp => new { id = dp.Id, name = dp.Name }),
                        cookingLevels = cookingLevels.Select(cl => new { id = cl.Id, name = cl.Name }),
                        healthGoals = healthGoals.Select(hg => new { id = hg.Id, name = hg.Name }),
                        lifestyles = lifestyles.Select(l => new { id = l.Id, name = l.Name })
                    }
                };
            }
            catch (Exception ex)
            {
                return new { error = ex.Message, stackTrace = ex.StackTrace };
            }
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.DietPlan)
                    .Include(u => u.CookingLevel)
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Userid == userId);

                Console.WriteLine($"User found: {user != null}");
                if (user == null)
                {
                    return null;
                }

                Console.WriteLine($"DietPlanId: {user.DietPlanId}, CookingLevelId: {user.CookingLevelId}");
                Console.WriteLine($"DietPlan loaded: {user.DietPlan != null}");
                Console.WriteLine($"CookingLevel loaded: {user.CookingLevel != null}");
                
                // Debug the actual values being set
                var dietPlanName = user.DietPlan?.Name ?? "Chưa cập nhật";
                var cookingLevelName = user.CookingLevel?.Name ?? "Chưa cập nhật";
                Console.WriteLine($"Final DietPlan name: {dietPlanName}");
                Console.WriteLine($"Final CookingLevel name: {cookingLevelName}");
                
                // If navigation properties are null, try to load them manually
                if (user.DietPlan == null && user.DietPlanId.HasValue)
                {
                    Console.WriteLine($"Trying to manually load DietPlan with ID: {user.DietPlanId.Value}");
                    var dietPlan = await _context.DietPlans.FindAsync(user.DietPlanId.Value);
                    if (dietPlan != null)
                    {
                        dietPlanName = dietPlan.Name;
                        Console.WriteLine($"Manually loaded DietPlan: {dietPlanName}");
                    }
                    else
                    {
                        Console.WriteLine($"DietPlan with ID {user.DietPlanId.Value} not found in database");
                    }
                }
                
                if (user.CookingLevel == null && user.CookingLevelId.HasValue)
                {
                    Console.WriteLine($"Trying to manually load CookingLevel with ID: {user.CookingLevelId.Value}");
                    var cookingLevel = await _context.CookingLevels.FindAsync(user.CookingLevelId.Value);
                    if (cookingLevel != null)
                    {
                        cookingLevelName = cookingLevel.Name;
                        Console.WriteLine($"Manually loaded CookingLevel: {cookingLevelName}");
                    }
                    else
                    {
                        Console.WriteLine($"CookingLevel with ID {user.CookingLevelId.Value} not found in database");
                    }
                }

                // Get health profile information (goals and activity level)
                var healthProfile = await _context.Healthprofiles
                    .Include(hp => hp.Healthgoal)
                    .Include(hp => hp.Lifestyle)
                    .FirstOrDefaultAsync(hp => hp.Userid == userId);

                Console.WriteLine($"Health profile for user {userId}: {(healthProfile != null ? "Found" : "Not found")}");
                if (healthProfile != null)
                {
                    Console.WriteLine($"Goal: {healthProfile.Healthgoal?.Name}");
                    Console.WriteLine($"Lifestyle: {healthProfile.Lifestyle?.Name}");
                }
                
                Console.WriteLine($"User DietPlan: {user.DietPlan?.Name ?? "NULL"} (ID: {user.DietPlanId})");
                Console.WriteLine($"User CookingLevel: {user.CookingLevel?.Name ?? "NULL"} (ID: {user.CookingLevelId})");

                var result = new UserProfileDto
                {
                    Id = user.Userid,
                    Fullname = user.Fullname ?? "",
                    Email = user.Email ?? "",
                    GenderId = user.GenderId,
                    Age = user.Age,
                    Height = user.Height,
                    Weight = user.Weight,
                    Country = user.Country,
                    AvatarUrl = user.AvatarUrl,
                    TargetWeight = (int)(user.TargetWeight ?? 0),
                    Gender = user.GenderId == 1 ? "Nam" : "Nữ",
                    DietPlan = dietPlanName,
                    CookingLevel = cookingLevelName,
                    Goal = healthProfile?.Healthgoal?.Name ?? "Chưa cập nhật",
                    OtherGoal = user.TargetWeight?.ToString(),
                    ActivityLevel = healthProfile?.Lifestyle?.Name ?? "Chưa cập nhật",
                    IsOnboardingCompleted = true,
                    TargetCalories = 2000,
                    RoleId = user.RoleId,
                    RoleName = user.Role?.Name ?? "User",
                    AccountType = user.RoleId == 3 ? "PRO" : "FREE",
                    IsEmailVerified = user.IsEmailVerified,
                    CreatedAt = user.Createdat,
                    UpdatedAt = user.Updatedat
                };

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Deactivate account method
        public async Task<bool> DeactivateAccountAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Set status to inactive instead of deleting
                user.Status = false;
 
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deactivating account: {ex.Message}");
                return false;
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
            catch (Exception)
            {
            }
        }

        public async Task<int> GetConsumedCaloriesAsync(int userId, DateTime date)
        {
            try
            {
                var consumedCalories = await _context.MealHistories
                    .Where(mh => mh.Userid == userId && mh.Date == DateOnly.FromDateTime(date))
                    .SumAsync(mh => mh.Calories ?? 0);
                
                return consumedCalories;
            }
            catch (Exception)
            {
                // Return 0 if there's an error
                return 0;
            }
        }

        public async Task<List<DietPlan>> GetAllDietPlansAsync()
        {
            return await _context.DietPlans.ToListAsync();
        }

        public async Task<List<CookingLevel>> GetAllCookingLevelsAsync()
        {
            return await _context.CookingLevels.ToListAsync();
        }

        public async Task<List<Healthgoal>> GetAllHealthGoalsAsync()
        {
            return await _context.Healthgoals.ToListAsync();
        }

        public async Task<List<Lifestyle>> GetAllLifestylesAsync()
        {
            return await _context.Lifestyles.ToListAsync();
        }
    }
}
