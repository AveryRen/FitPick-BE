using FitPick_EXE201.Models.Entities;

namespace FitPick_EXE201.Repositories.Interface
{
    public interface IPayosPaymentRepo
    {
        Task<PayosPayment?> GetByIdAsync(int paymentId);
        Task<IEnumerable<PayosPayment>> GetByUserIdAsync(int userId);
        Task<IEnumerable<PayosPayment>> GetAllAsync();
        Task<bool> DeleteAsync(int paymentId);
    }
}
