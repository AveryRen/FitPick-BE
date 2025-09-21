using System;
using System.Threading.Tasks;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class UserPremiumService
    {
        private readonly IUserPremiumRepo _userPremiumRepo;
        private const int PremiumRoleId = 3;

        public UserPremiumService(IUserPremiumRepo userPremiumRepo)
        {
            _userPremiumRepo = userPremiumRepo;
        }

        public async Task<bool> UpgradeUserRoleToPremiumAsync(int userId)
        {
            return await _userPremiumRepo.UpdateUserRoleAsync(userId, PremiumRoleId);
        }
    }
}
