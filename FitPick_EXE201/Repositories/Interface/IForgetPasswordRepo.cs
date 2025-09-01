using FitPick_EXE201.Models.DTOs;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IForgetPasswordRepo
    {
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
