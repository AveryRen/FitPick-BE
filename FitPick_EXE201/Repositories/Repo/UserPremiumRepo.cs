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
            user.Updatedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

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

        // Thêm giao d?ch m?i
        public async Task<bool> InsertPaymentAsync(PayosPayment payment)
        {
            _context.PayosPayments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        // C?p nh?t tr?ng thái giao d?ch
        public async Task<bool> UpdatePaymentStatusAsync(
            long orderCode,
            string status,
            DateTime? transactionTime,
            decimal? amount = null,
            string? description = null
)
        {
            var payment = await _context.PayosPayments
                .FirstOrDefaultAsync(p => p.OrderCode == orderCode);

            if (payment == null) return false;

            // C?p nh?t tr?ng thái
            payment.Status = status;

            // N?u có th?i gian giao d?ch (thanh toán thành công)
            if (transactionTime.HasValue)
                payment.TransactionDatetime = DateTime.SpecifyKind(transactionTime.Value, DateTimeKind.Unspecified);
            else if (status.Equals("PAID", StringComparison.OrdinalIgnoreCase) && payment.TransactionDatetime == null)
                payment.TransactionDatetime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            // C?p nh?t s? ti?n/description n?u callback g?i l?i
            if (amount.HasValue)
                payment.Amount = amount.Value;

            if (!string.IsNullOrWhiteSpace(description))
                payment.Description = description;

            // Luôn c?p nh?t Updatedat
            payment.Updatedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            _context.PayosPayments.Update(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PayosPayment?> GetPaymentByOrderCodeAsync(long orderCode)
        {
            return await _context.PayosPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderCode == orderCode);
        }
    }
}
