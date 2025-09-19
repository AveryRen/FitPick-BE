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
        public async Task<PayosPayment> CreatePaymentAsync(PayosPayment payment)
        {
            var createdPayment = await _repository.CreatePaymentAsync(payment);
            return createdPayment;
        }

        public async Task<IEnumerable<PayosPayment>> GetAllPaymentsAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}