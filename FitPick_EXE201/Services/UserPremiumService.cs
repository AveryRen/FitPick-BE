using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;

namespace FitPick_EXE201.Services
{
    public class UserPremiumService
    {
        private readonly IUserPremiumRepo _repo;
        private const int PremiumRoleId = 3;
        private const int DefaultRoleId = 2; 

        public UserPremiumService(IUserPremiumRepo repo)
        {
            _repo = repo;
        }

        // ==================== USER ====================

        /// <summary>
        /// Nâng cấp user lên Premium
        /// </summary>
        public async Task<bool> UpgradeUserRoleToPremiumAsync(int userId)
        {
            return await _repo.UpdateUserRoleAsync(userId, PremiumRoleId);
        }

        /// <summary>
        /// Hạ cấp user về role mặc định
        /// </summary>
        public async Task<bool> DowngradeUserAsync(int userId)
        {
            return await _repo.DowngradeUserRoleAsync(userId, DefaultRoleId);
        }

        /// <summary>
        /// Kiểm tra user có phải Premium hay không
        /// </summary>
        public Task<bool> IsUserPremiumAsync(int userId)
        {
            return _repo.IsUserPremiumAsync(userId);
        }

        /// <summary>
        /// Lấy thông tin user theo Id
        /// </summary>
        public Task<User?> GetUserByIdAsync(int userId)
        {
            return _repo.GetUserByIdAsync(userId);
        }

        // ==================== PAYMENT ====================

        /// <summary>
        /// Lưu giao dịch mới (thường khi tạo link thanh toán)
        /// </summary>
        public async Task<bool> CreatePaymentAsync(PayosPayment payment)
        {
            if (payment.Createdat.HasValue)
            {
                payment.Createdat = DateTime.SpecifyKind(payment.Createdat.Value, DateTimeKind.Unspecified);
            }
            payment.Status ??= "PENDING";
            return await _repo.InsertPaymentAsync(payment);
        }


        /// <summary>
        /// Cập nhật trạng thái giao dịch khi PayOS callback
        /// </summary>
        public Task<bool> UpdatePaymentStatusAsync(long orderCode, string status, DateTime? transactionTime = null)
        {
            return _repo.UpdatePaymentStatusAsync(orderCode, status, transactionTime);
        }
        public async Task<PayosPayment?> GetPaymentByOrderCodeAsync(long orderCode)
        {
            return await _repo.GetPaymentByOrderCodeAsync(orderCode);
        }
    }
}
