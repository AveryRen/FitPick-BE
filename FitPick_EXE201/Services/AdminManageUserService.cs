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

        // Create new user
        public async Task<User> CreateUserAsync(User user)
        {
            user.Status = user.Status ?? true;
            return await _userRepo.CreateUserAsync(user);
        }

        public async Task<bool> UpdateUserAsync(int id, AdminUserDetailDto dto)
        {
            var user = await _userRepo.GetUserByIdForAdminAsync(id);
            if (user == null) return false;

            // Chỉ cập nhật những field cho phép
            if (!string.IsNullOrWhiteSpace(dto.Fullname))
                user.Fullname = dto.Fullname;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (dto.GenderId > 0) // GenderId là int, internal set, >0 mới cập nhật
                user.GenderId = dto.GenderId;

            if (dto.Age.HasValue)
                user.Age = dto.Age;

            if (dto.Height.HasValue)
                user.Height = dto.Height;

            if (dto.Weight.HasValue)
                user.Weight = dto.Weight;

            if (!string.IsNullOrWhiteSpace(dto.Country))
                user.Country = dto.Country;

            if (!string.IsNullOrWhiteSpace(dto.City))
                user.City = dto.City;

            if (dto.RoleId.HasValue)
                user.RoleId = dto.RoleId;

            if (dto.Status.HasValue)
                user.Status = dto.Status;
 
            return await _userRepo.UpdateUserAsync(id, user);
        }


        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepo.DeleteUserAsync(id);
        }
    }
}
