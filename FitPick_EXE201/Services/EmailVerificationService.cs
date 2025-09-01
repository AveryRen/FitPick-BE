using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class EmailVerificationService
    {
        private readonly IEmailVerificationRepo _repo;

        public EmailVerificationService(IEmailVerificationRepo repo)
        {
            _repo = repo;
        }

        public Task<bool> RequestEmailVerificationAsync(string email)
        {
            return _repo.RequestEmailVerificationAsync(email);
        }

        public Task<bool> VerifyEmailAsync(string email, string code)
        {
            return _repo.VerifyEmailAsync(email, code);
        }
    }
}