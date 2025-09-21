using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class UserPremiumRepo : IUserPremiumRepo
    {
        private readonly FitPickContext _context;
        
        public UserPremiumRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, int newRoleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
            if (user == null) return false;

            user.RoleId = newRoleId;
            user.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
