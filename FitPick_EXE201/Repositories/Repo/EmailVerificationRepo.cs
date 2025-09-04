using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace FitPick_EXE201.Repositories.Repo
{
    public class EmailVerificationRepo : IEmailVerificationRepo
    {
        private readonly FitPickContext _context;
        private readonly IEmailService _emailService;

        // Lưu mã xác thực tạm
        private static readonly ConcurrentDictionary<string, EmailVerifyInfo> _verifyStore = new();

        public EmailVerificationRepo(FitPickContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> RequestEmailVerificationAsync(string email)
        {
            var account = await _context.Users.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null) return false;

            var code = Guid.NewGuid().ToString("N")[..6].ToUpper();

            _verifyStore[email] = new EmailVerifyInfo
            {
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _emailService.SendAsync(email, "Email Verification Code", $"Your verification code is: {code}");
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
            if (!_verifyStore.TryGetValue(email, out var info))
                return false;

            if (info.Code != code || info.ExpiresAt < DateTime.UtcNow)
                return false;

            var account = await _context.Users.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null) return false;

            account.IsEmailVerified = true;
            account.Status = true;
            account.Updatedat = DateTime.Now;

            _verifyStore.TryRemove(email, out _);

            await _context.SaveChangesAsync();
            return true;
        }

    }
}