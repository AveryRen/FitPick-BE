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

            // Cập nhật trạng thái
            payment.Status = status;

            // Nếu có thời gian giao dịch (thanh toán thành công)
            if (transactionTime.HasValue)
                payment.TransactionDatetime = DateTime.SpecifyKind(transactionTime.Value, DateTimeKind.Unspecified);
            else if (status.Equals("PAID", StringComparison.OrdinalIgnoreCase) && payment.TransactionDatetime == null)
                payment.TransactionDatetime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // Cập nhật số tiền/description nếu callback gửi lại
            if (amount.HasValue)
                payment.Amount = amount.Value;

            if (!string.IsNullOrWhiteSpace(description))
                payment.Description = description;

            // Luôn cập nhật Updatedat
            payment.Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

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
