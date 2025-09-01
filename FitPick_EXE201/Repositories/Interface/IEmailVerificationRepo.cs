namespace FitPick_EXE201.Repositories.Interface
{
    public interface IEmailVerificationRepo
    {
        Task<bool> RequestEmailVerificationAsync(string email);
        Task<bool> VerifyEmailAsync(string email, string code);
    }
}
