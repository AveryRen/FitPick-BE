using FitPick_EXE201.Models.Requests;

namespace FitPick_EXE201.Services
{
    public interface ISettingsService
    {
        Task<string> GetPrivacyPolicyContentAsync();
        Task<string> GetTermsOfServiceContentAsync();
        Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}
