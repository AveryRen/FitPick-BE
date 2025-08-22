using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class UserService
    {
        private readonly IUserRepo _userRepo;

        public UserService(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<UserProfileDto?> GetUserByIdAsync(int id)
        {
            return await _userRepo.GetUserByIdAsync(id);
        }

        public async Task<UpdateUserProfileDto?> UpdateProfileAsync(int userId, UpdateUserProfileRequest request)
        {
            return await _userRepo.UpdateProfileAsync(userId, request);
        } 
        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
             string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            return await _userRepo.ChangePasswordAsync(userId, hashedPassword);
        }

        public async Task<bool> ChangeAvatarAsync(int userId, string avatarUrl)
        {
            return await _userRepo.ChangeAvatarAsync(userId, avatarUrl);
        }
    }
}