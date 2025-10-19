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
        /// Nâng c?p user lên Premium
        /// </summary>
        public async Task<bool> UpgradeUserRoleToPremiumAsync(int userId)
        {
            return await _repo.UpdateUserRoleAsync(userId, PremiumRoleId);
        }

        /// <summary>
        /// H? c?p user v? role m?c d?nh
        /// </summary>
        public async Task<bool> DowngradeUserAsync(int userId)
        {
            return await _repo.DowngradeUserRoleAsync(userId, DefaultRoleId);
        }

        /// <summary>
        /// Ki?m tra user có ph?i Premium hay không
        /// </summary>
        public Task<bool> IsUserPremiumAsync(int userId)
        {
            return _repo.IsUserPremiumAsync(userId);
        }

        /// <summary>
        /// L?y thông tin user theo Id
        /// </summary>
        public Task<User?> GetUserByIdAsync(int userId)
        {
            return _repo.GetUserByIdAsync(userId);
        }

        // ==================== PAYMENT ====================

        /// <summary>
        /// Luu giao d?ch m?i (thu?ng khi t?o link thanh toán)
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
        /// C?p nh?t tr?ng thái giao d?ch khi PayOS callback
        /// </summary>
        public Task<bool> UpdatePaymentStatusAsync(
            long orderCode,
            string status,
            DateTime? transactionTime = null,
            decimal? amount = null,
            string? description = null
        )
        {
            return _repo.UpdatePaymentStatusAsync(orderCode, status, transactionTime, amount, description);
        }

        public async Task<PayosPayment?> GetPaymentByOrderCodeAsync(long orderCode)
        {
            return await _repo.GetPaymentByOrderCodeAsync(orderCode);
        }
    }
}
