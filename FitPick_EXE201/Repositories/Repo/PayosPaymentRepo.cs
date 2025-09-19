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
        public async Task<PayosPayment> CreatePaymentAsync(PayosPayment payment)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api-merchant.payos.vn");
            client.DefaultRequestHeaders.Add("x-partner-code", _config["Payos:PartnerCode"]);
            client.DefaultRequestHeaders.Add("x-client-idx-api-key", _config["Payos:ApiKey"]);

            // Tạo signature
            var signatureData = $"amount={payment.Amount}&cancelUrl={_config["Payos:CancelUrl"]}&description={payment.Description}&orderCode={payment.OrderCode}&returnUrl={_config["Payos:ReturnUrl"]}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config["Payos:ChecksumKey"]));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureData));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            var requestBody = new
            {
                orderCode = payment.OrderCode,
                amount = payment.Amount,
                description = payment.Description,
                buyerEmail = payment.User.Email,
                buyerName = payment.User.Fullname,
                cancelUrl = _config["Payos:CancelUrl"],
                returnUrl = _config["Payos:ReturnUrl"],
                expiredAt = ((DateTimeOffset)DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds(),
                signature
            };

            var response = await client.PostAsJsonAsync("/v2/payment-requests", requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PayosPaymentResponse>();

            // Lưu vào DB
            payment.CheckoutUrl = result.Data.CheckoutUrl;
            payment.Status = result.Data.Status;
            payment.Createdat = DateTime.UtcNow;
            payment.Updatedat = DateTime.UtcNow;

            _context.PayosPayments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        } 
    }
} 