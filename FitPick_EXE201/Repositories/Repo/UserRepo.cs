using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class UserRepo : IUserRepo
    {
        private readonly FitPickContext _context;

        public UserRepo(FitPickContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == id);
            if (user == null) return null;

            return new UserProfileDto
            {
                Fullname = user.Fullname,
                Email = user.Email,
                GenderId = user.GenderId,
                Age = user.Age,
                Height = user.Height,
                Weight = user.Weight,
                Country = user.Country,
                AvatarUrl = user.AvatarUrl
            };
        }
        public async Task<UpdateUserProfileDto?> UpdateProfileAsync(int userId, UpdateUserProfileRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
            if (user == null) return null;

            user.Fullname = request.Fullname ?? user.Fullname;
            user.GenderId = request.GenderId ?? user.GenderId;
            user.Age = request.Age ?? user.Age;
            user.Height = request.Height ?? user.Height;
            user.Weight = request.Weight ?? user.Weight;
            user.Country = request.Country ?? user.Country;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new UpdateUserProfileDto
            {
                Fullname = user.Fullname,
                GenderId = user.GenderId,
                Age = user.Age,
                Height = user.Height,
                Weight = user.Weight,
                Country = user.Country,
                AvatarUrl = user.AvatarUrl  
            };
        }


        public async Task<bool> ChangePasswordAsync(int userId, string newPasswordHash)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
            if (user == null) return false;

            user.Passwordhash = newPasswordHash;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeAvatarAsync(int userId, string avatarUrl)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
            if (user == null) return false;

            user.AvatarUrl = avatarUrl;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
