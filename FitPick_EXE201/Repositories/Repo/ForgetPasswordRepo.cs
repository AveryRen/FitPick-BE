using FitPick_EXE201.Data;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace FitPick_EXE201.Repositories.Repo
{
    public class ForgetPasswordRepo : IForgetPasswordRepo
    {
        private readonly FitPickContext _context;
        private readonly IEmailService _emailService;
        private static readonly ConcurrentDictionary<string, PasswordResetInfo> _resetStore = new();

        public ForgetPasswordRepo(FitPickContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var account = await _context.Users.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null) return false;

            var code = Guid.NewGuid().ToString("N")[..6].ToUpper();

            _resetStore[email] = new PasswordResetInfo
            {
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _emailService.SendAsync(email, "Reset Code", $"Your reset code is: {code}");

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (!_resetStore.TryGetValue(dto.Email, out var info))
                return false;

            if (info.Code != dto.VerificationCode || info.ExpiresAt < DateTime.UtcNow)
                return false;

            var account = await _context.Users.FirstOrDefaultAsync(a => a.Email == dto.Email);
            if (account == null) return false;
            
            account.Passwordhash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _resetStore.TryRemove(dto.Email, out _);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}