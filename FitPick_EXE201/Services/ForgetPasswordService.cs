using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class ForgetPasswordService
    {
        private readonly IForgetPasswordRepo _repo;

        public ForgetPasswordService(IForgetPasswordRepo repo)
        {
            _repo = repo;
        }

        public Task<bool> RequestPasswordResetAsync(string email)
        {
            return _repo.RequestPasswordResetAsync(email);
        }

        public Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            return _repo.ResetPasswordAsync(dto);
        }
    }
}