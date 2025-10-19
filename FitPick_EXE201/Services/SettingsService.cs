using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<string> GetPrivacyPolicyContentAsync()
        {
            return await _settingsRepository.GetPrivacyPolicyContentAsync();
        }

        public async Task<string> GetTermsOfServiceContentAsync()
        {
            return await _settingsRepository.GetTermsOfServiceContentAsync();
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request)
        {
            try
            {
                var user = await _settingsRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Update user profile fields
                if (!string.IsNullOrEmpty(request.Fullname))
                    user.Fullname = request.Fullname;
                if (request.GenderId.HasValue)
                    user.GenderId = request.GenderId;
                if (request.Age.HasValue)
                    user.Age = request.Age;
                if (request.Height.HasValue)
                    user.Height = request.Height;
                if (request.Weight.HasValue)
                    user.Weight = request.Weight;
                if (!string.IsNullOrEmpty(request.Country))
                    user.Country = request.Country;

                user.Updatedat = DateTime.Now;

                return await _settingsRepository.UpdateUserAsync(user);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _settingsRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Verify old password
                if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Passwordhash))
                {
                    return false;
                }

                // Update password
                user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.Updatedat = DateTime.Now;

                return await _settingsRepository.UpdateUserAsync(user);
            }
            catch
            {
                return false;
            }
        }
    }
}
