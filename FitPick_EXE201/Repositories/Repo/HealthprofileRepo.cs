using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class HealthprofileRepo : BaseRepo<Healthprofile, int>, IHealthprofileRepo
    {
        private readonly FitPickContext _context;
        public HealthprofileRepo(FitPickContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Healthprofile?> GetByUserIdAsync(int id)
        {
            return await _context.Healthprofiles
                .Include(h => h.Lifestyle)
                .Include(h => h.Healthgoal)
                .FirstOrDefaultAsync(h => h.Userid == id);
        }
        public async Task<ProgressDto?> GetUserProgressAsync(int userId)
        {
            var profile = await _context.Healthprofiles
                .Include(h => h.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Userid == userId && h.Status == true);

            if (profile == null) return null;

            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);
            double currentCalories = 0;

            if (_context.MealHistories != null)
            {
                currentCalories = await _context.MealHistories
                    .Where(m => m.Userid == userId
                             && m.ConsumedAt.HasValue
                             && m.ConsumedAt.Value.Date == today)
                    .SumAsync(m => (double?)(m.Calories ?? 0)) ?? 0;
            }

            return new ProgressDto
            {
                CurrentWeight = profile.User?.Weight,
                TargetWeight = profile.Targetweight,
                CurrentCalories = currentCalories,
                TargetCalories = profile.Targetcalories
            };
        }
        public async Task<UserGoalDto?> GetUserGoalAsync(int userId)
        {
            var profile = await _context.Healthprofiles
                .Include(h => h.Healthgoal)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Userid == userId && h.Status == true);

            if (profile == null) return null;

            return new UserGoalDto
            {
                UserId = (int)profile.Userid,
                TargetWeight = profile.Targetweight,
                TargetCalories = profile.Targetcalories,
                GoalName = profile.Healthgoal?.Name
            };
        }
    }
}
