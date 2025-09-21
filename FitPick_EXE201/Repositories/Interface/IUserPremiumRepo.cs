using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IUserPremiumRepo
    {  
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserRoleAsync(int userId, int newRoleId);
    }
}
