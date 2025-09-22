using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IUserPremiumRepo
    {
        //User
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserRoleAsync(int userId, int newRoleId);
        Task<bool> DowngradeUserRoleAsync(int userId, int defaultRoleId);  
        Task<bool> IsUserPremiumAsync(int userId);

        //Payment 
        Task<PayosPayment?> GetPaymentByOrderCodeAsync(long orderCode);
        Task<bool> InsertPaymentAsync(PayosPayment payment);
        Task<bool> UpdatePaymentStatusAsync(long orderCode, string status, DateTime? transactionTime);
    }
}
