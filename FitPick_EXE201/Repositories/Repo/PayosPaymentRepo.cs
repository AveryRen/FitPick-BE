using FitPick_EXE201.Data;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace FitPick_EXE201.Repositories.Repo
{
    public class PayosPaymentRepo : IPayosPaymentRepo
    {
        private readonly FitPickContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public PayosPaymentRepo(FitPickContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }
        public async Task<IEnumerable<PayosPayment>> GetAllAsync()
        {
            return await _context.PayosPayments
                .Include(p => p.User)
                .OrderByDescending(p => p.Createdat)
                .ToListAsync();
        }
        public async Task<PayosPayment?> GetByIdAsync(int paymentId)
        {
            return await _context.PayosPayments
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Paymentid == paymentId);
        }

        public async Task<IEnumerable<PayosPayment>> GetByUserIdAsync(int userId)
        {
            return await _context.PayosPayments
                .Where(p => p.Userid == userId)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int paymentId)
        {
            var payment = await _context.PayosPayments.FindAsync(paymentId);
            if (payment == null) return false;

            _context.PayosPayments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        } 
    }
} 