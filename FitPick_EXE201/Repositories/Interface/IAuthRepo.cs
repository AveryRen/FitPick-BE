using FitPick_EXE201.Models.Entities;
using System.Security.Principal;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IAuthRepo
    {
        Task<User> GetAccountByEmailAsync(string email);
        Task<User> GetAccountByCredentialsAsync(string email, string password);
        Task AddAsync(User account);
        Task UpdateAsync(User account);
    }
}
