using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface ISettingsRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserAsync(User user);
        Task<string> GetPrivacyPolicyContentAsync();
        Task<string> GetTermsOfServiceContentAsync();
    }
}
