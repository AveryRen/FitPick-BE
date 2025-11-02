using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Services
{
    public class PayosPaymentService
    {
        private readonly IPayosPaymentRepo _repository;

        public PayosPaymentService(IPayosPaymentRepo repository)
        {
            _repository = repository;
        }

        public async Task<PayosPayment?> GetPaymentByIdAsync(int paymentId)
        {
            return await _repository.GetByIdAsync(paymentId);
        }

        public async Task<IEnumerable<PayosPayment>> GetPaymentsByUserIdAsync(int userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }
        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            return await _repository.DeleteAsync(paymentId);
        }
        public async Task<IEnumerable<PayosPayment>> GetAllPaymentsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<(List<PayosPayment> items, int totalCount)> GetAllPaymentsPagedAsync(int page, int pageSize, string? search, string? status, int? userId)
        {
            return await _repository.GetAllPagedAsync(page, pageSize, search, status, userId);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status)
        {
            return await _repository.UpdateStatusAsync(paymentId, status);
        }
    }
}
