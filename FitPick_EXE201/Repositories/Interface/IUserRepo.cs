using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IUserRepo
    {
        Task<UserProfileDto?> GetUserByIdAsync(int id);
        Task<UserAIProfileDto?> GetUserAIProfileAsync(int id); // ✅ thêm mới
        Task<UpdateUserProfileDto?> UpdateProfileAsync(int userId, UpdateUserProfileRequest request);
        Task<bool> ChangePasswordAsync(int userId, string newPasswordHash);
        Task<bool> ChangeAvatarAsync(int userId, string avatarUrl);
    }
}
