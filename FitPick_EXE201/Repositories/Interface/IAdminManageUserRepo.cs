using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IAdminManageUserRepo
    {
        Task<IEnumerable<User>> GetAllUsersAsync(
            int currentAdminId,
            string? searchKeyword,
            string? sortBy,
            bool sortDesc,
            int? genderId,
            int? roleId,
            bool? status
        );
        Task<AdminUserDetailDto?> GetUserByIdForAdminAsync(int id);
        Task<User?> GetUserEntityByIdAsync(int id);

        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);

        Task<User?> GetByEmailAsync(string email);

    }
}
