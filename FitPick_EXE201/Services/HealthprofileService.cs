using AutoMapper;
using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class HealthprofileService
    {
        private readonly IHealthprofileRepo _repo;
        //private readonly SubjectService _subjectService;
        private readonly IMapper _mapper;
        private readonly FitPickContext _context;

        public HealthprofileService(IHealthprofileRepo repo, IMapper mapper, FitPickContext context)
        {
            _context = context;
            _repo = repo;
            _mapper = mapper;
            //_subjectService = subjectService;
        }

        public async Task<HealthprofileDTO?> CreateHealthprofileAsync(int userId, HealthprofileRequest request)
        {
            var healthprofile = _mapper.Map<Healthprofile>(request);
            healthprofile.Userid = userId; // Gán từ JWT
            healthprofile.Status = true;

            // ====== TÍNH TARGET CALORIES ======
            double? calories = null;

            if (request.Height.HasValue && request.Weight.HasValue && request.Age.HasValue)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Userid == userId);

                double height = request.Height.Value;
                double weight = request.Weight.Value;
                int age = request.Age.Value;
                double bmr = 0;

                // Lấy gender, nếu null thì mặc định 1 (Male)
                int gender = user?.GenderId ?? 1;

                // Công thức BMR
                if (gender == 1) // Male
                {
                    bmr = 88.362 + (13.397 * weight) + (4.799 * height) - (5.677 * age);
                }
                else if (gender == 2) // Female
                {
                    bmr = 447.593 + (9.247 * weight) + (3.098 * height) - (4.330 * age);
                }

                // Hệ số hoạt động
                double multiplier = 1.2;
                if (request.Lifestyleid.HasValue)
                {
                    var lifestyle = await _context.Lifestyles
                        .FirstOrDefaultAsync(l => l.Id == request.Lifestyleid.Value);
                    if (lifestyle?.Multiplier != null)
                    {
                        multiplier = (double)lifestyle.Multiplier;
                    }
                }

                calories = bmr * multiplier;

                // Điều chỉnh theo healthgoal
                if (request.Healthgoalid.HasValue)
                {
                    var goal = await _context.Healthgoals
                        .FirstOrDefaultAsync(g => g.Id == request.Healthgoalid.Value);
                    if (goal?.CalorieAdjustment != null)
                    {
                        calories += (double)goal.CalorieAdjustment;
                    }
                }
            }

            if (calories.HasValue)
            {
                healthprofile.Targetcalories = (int)Math.Round(calories.Value);
            }

            // ====== CREATE PROFILE ======
            var createdProfile = await _repo.CreateAsync(healthprofile);

            var fullProfile = await _context.Healthprofiles
                .Include(h => h.Lifestyle)
                .Include(h => h.Healthgoal)
                .FirstOrDefaultAsync(h => h.Profileid == createdProfile.Profileid);

            var healthprofileDto = _mapper.Map<HealthprofileDTO>(fullProfile);

            // ====== MAP ALLERGY DETAILS ======
            if (createdProfile.Allergies != null && createdProfile.Allergies.Any(id => id != 0))
            {
                var allergyIngredients = await _context.Ingredients
                    .Where(i => createdProfile.Allergies.Contains(i.Ingredientid))
                    .ToListAsync();

                healthprofileDto.AllergyDetails = _mapper.Map<List<IngredientDTO>>(allergyIngredients);
            }

            // Chronicdiseases
            if (createdProfile.Chronicdiseases != null && createdProfile.Chronicdiseases.Any(id => id != 0))
            {
                var chronicIngredients = await _context.Ingredients
                    .Where(i => createdProfile.Chronicdiseases.Contains(i.Ingredientid))
                    .ToListAsync();

                healthprofileDto.ChronicdiseasesDetails = _mapper.Map<List<IngredientDTO>>(chronicIngredients);
            }

            // Religiondiet
            if (createdProfile.Religiondiet != null && createdProfile.Religiondiet.Any(id => id != 0))
            {
                var religionIngredients = await _context.Ingredients
                    .Where(i => createdProfile.Religiondiet.Contains(i.Ingredientid))
                    .ToListAsync();

                healthprofileDto.ReligiondietDetails = _mapper.Map<List<IngredientDTO>>(religionIngredients);
            }

            // Dietarypreferences
            if (createdProfile.Dietarypreferences != null && createdProfile.Dietarypreferences.Any(id => id != 0))
            {
                var dietaryIngredients = await _context.Ingredients
                    .Where(i => createdProfile.Dietarypreferences.Contains(i.Ingredientid))
                    .ToListAsync();

                healthprofileDto.DietarypreferencesDetails = _mapper.Map<List<IngredientDTO>>(dietaryIngredients);
            }

            return healthprofileDto;
        }


        public async Task<HealthprofileDTO?> GetByUserIdAsync(int userId)
        {
            var profile = await _repo.GetByUserIdAsync(userId);

            if (profile == null)
                return null;

            var healthprofileDto = _mapper.Map<HealthprofileDTO>(profile);

            // Allergy Details
            if (profile.Allergies != null && profile.Allergies.Any(id => id != 0))
            {
                var allergyIngredients = await _context.Ingredients
                    .Where(i => profile.Allergies.Contains(i.Ingredientid))
                    .ToListAsync();

                healthprofileDto.AllergyDetails = _mapper.Map<List<IngredientDTO>>(allergyIngredients);
            }

            // Chronic Diseases Details
            if (profile.Chronicdiseases != null && profile.Chronicdiseases.Any(id => id != 0))
            {
                var chronicIngredients = await _context.Ingredients
                    .Where(i => profile.Chronicdiseases.Contains(i.Ingredientid))
                    .ToListAsync();

                healthprofileDto.ChronicdiseasesDetails = _mapper.Map<List<IngredientDTO>>(chronicIngredients);
            }

            // Religion Diet Details
            if (profile.Religiondiet != null && profile.Religiondiet.Any(id => id != 0))
            {
                var religionIngredients = await _context.Ingredients
                    .Where(i => profile.Religiondiet.Contains(i.Ingredientid))
                    .ToListAsync();

                healthprofileDto.ReligiondietDetails = _mapper.Map<List<IngredientDTO>>(religionIngredients);
            }

            // Dietary Preferences Details
            if (profile.Dietarypreferences != null && profile.Dietarypreferences.Any(id => id != 0))
            {
                var dietaryIngredients = await _context.Ingredients
                    .Where(i => profile.Dietarypreferences.Contains(i.Ingredientid))
                    .ToListAsync();

                healthprofileDto.DietarypreferencesDetails = _mapper.Map<List<IngredientDTO>>(dietaryIngredients);
            }

            return healthprofileDto;
        }
        public async Task<ProgressDto?> GetUserProgressAsync(int userId)
        {
            return await _repo.GetUserProgressAsync(userId);
        }
        public async Task<UserGoalDto?> GetUserGoalAsync(int userId)
        {
            return await _repo.GetUserGoalAsync(userId);
        }
    }
}
