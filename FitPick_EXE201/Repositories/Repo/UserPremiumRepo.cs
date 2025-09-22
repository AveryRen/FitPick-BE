using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Repositories.Repo
{
    public class UserPremiumRepo : IUserPremiumRepo
    {
        private readonly FitPickContext _context;

        public UserPremiumRepo(FitPickContext context)
        {
            _context = context;
        }

        // ==================== USER ====================
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Userid == userId);
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, int newRoleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == userId);
            if (user == null) return false;

            user.RoleId = newRoleId;
            user.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DowngradeUserRoleAsync(int userId, int defaultRoleId)
        {
            return await UpdateUserRoleAsync(userId, defaultRoleId);
        }

        public async Task<bool> IsUserPremiumAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Userid == userId);
            return user != null && user.RoleId == 3;
        }

        // ==================== PAYMENT ====================

        // Thêm giao dịch mới
        public async Task<bool> InsertPaymentAsync(PayosPayment payment)
        {
            _context.PayosPayments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        // Cập nhật trạng thái giao dịch
        public async Task<bool> UpdatePaymentStatusAsync(long orderCode, string status, DateTime? transactionTime)
        {
            var payment = await _context.PayosPayments.FirstOrDefaultAsync(p => p.OrderCode == orderCode);
            if (payment == null) return false;

            payment.Status = status;
            if (transactionTime.HasValue)
                payment.TransactionDatetime = transactionTime;
            payment.Updatedat = DateTime.UtcNow;

            _context.PayosPayments.Update(payment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
