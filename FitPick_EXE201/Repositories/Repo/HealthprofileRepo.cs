using FitPick_EXE201.Data;
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
    }
}
