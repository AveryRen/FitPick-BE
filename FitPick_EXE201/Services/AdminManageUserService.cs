using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class AdminManageUserService
    {
        private readonly IAdminManageUserRepo _userRepo;

        public AdminManageUserService(IAdminManageUserRepo userRepo)
        {
            _userRepo = userRepo;
        } 
        public async Task<IEnumerable<User>> GetAllUsersAsync(
            int currentAdminId,
            string? searchKeyword,
            string? sortBy,
            bool sortDesc,
            int? genderId,
            int? roleId,
            bool? status
        )
        {
            return await _userRepo.GetAllUsersAsync(
                currentAdminId,
                searchKeyword,
                sortBy,
                sortDesc,
                genderId,
                roleId,
                status
            );
        } 
        public async Task<AdminUserDetailDto?> GetUserByIdForAdminAsync(int id)
        {
            return await _userRepo.GetUserByIdForAdminAsync(id);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // Check trùng email
            var existingUser = await _userRepo.GetByEmailAsync(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email đã tồn tại.");
            }

             if (user.RoleId <= 0)
            {
                user.RoleId = 2; 
            }

             if (!string.IsNullOrWhiteSpace(user.Passwordhash))
            {
                user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(user.Passwordhash);
            }

             user.Status = user.Status ?? true;

             user.Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            user.Updatedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

             return await _userRepo.CreateUserAsync(user);
        }

        public async Task<bool> UpdateUserAsync(int id, AdminUserDetailDto dto)
        {
            var user = await _userRepo.GetUserEntityByIdAsync(id);
            if (user == null) return false;

            // Check email trùng (ngoại trừ chính user đang update)
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
                if (existingUser != null && existingUser.Userid != id)
                    throw new InvalidOperationException($"Email '{dto.Email}' is already in use by another user.");

                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Fullname))
                user.Fullname = dto.Fullname;

            if (dto.Age.HasValue)
                user.Age = dto.Age.Value;

            if (dto.Height.HasValue)
                user.Height = dto.Height.Value;

            if (dto.Weight.HasValue)
                user.Weight = dto.Weight.Value;

            if (!string.IsNullOrWhiteSpace(dto.Country))
                user.Country = dto.Country;

            if (!string.IsNullOrWhiteSpace(dto.City))
                user.City = dto.City;

            if (dto.GenderId.HasValue)
                user.GenderId = dto.GenderId.Value;

            if (dto.RoleId.HasValue)
                user.RoleId = dto.RoleId.Value;

            if (dto.Status.HasValue)
                user.Status = dto.Status.Value;

            user.Updatedat = DateTime.Now;

            return await _userRepo.UpdateUserAsync(user);
        }


        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepo.DeleteUserAsync(id);
        }
    }
}
