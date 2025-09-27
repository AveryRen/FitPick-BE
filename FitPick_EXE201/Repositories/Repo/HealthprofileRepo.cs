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
        //public async Task<ProgressDto?> GetUserProgressAsync(int userId)
        //{
        //    // Lấy Healthprofile active của user
        //    var profile = await _context.Healthprofiles
        //        .Include(h => h.User)
        //        .Include(h => h.Healthgoal)   // nếu trong Healthgoal có TargetWeight/TargetCalories
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(h => h.Userid == userId && h.Status == true);

        //    if (profile == null) return null;

        //    // Lấy tổng calories đã ăn hôm nay (nếu có bảng MealHistories)
        //    var today = DateTime.UtcNow.Date;
        //    double currentCalories = 0;

        //    if (_context.MealHistories != null)
        //    {
        //        currentCalories = await _context.MealHistories
        //            .Where(m => m.Userid == userId && m.ConsumedAt.Date == today)
        //            .SumAsync(m => (double?)m.TotalCalories) ?? 0;
        //    }

        //    // Map vào ProgressDto
        //    return new ProgressDto
        //    {
        //        CurrentWeight = profile.User?.Weight,
        //        TargetWeight = profile.Healthgoal?.TargetWeight,   // nếu cột này tồn tại
        //        CurrentCalories = currentCalories,
        //        TargetCalories = profile.Targetcalories
        //    };
        }
    } 
