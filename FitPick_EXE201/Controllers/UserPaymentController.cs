using System.Security.Claims;
using System.Text.Json;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Services;
using FitPick_EXE201.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;

namespace FitPick_EXE201.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class UserPaymentController : ControllerBase
    {
        private readonly PayOS _payOS;
        private readonly string _returnUrl;
        private readonly string _webhookUrl;
        private readonly UserPremiumService _premiumService;

        public UserPaymentController(IOptions<PayOSSettings> options, UserPremiumService premiumService)
        {
            var opts = options.Value;
            _payOS = new PayOS(opts.ClientId, opts.ApiKey, opts.ChecksumKey);
            _returnUrl = opts.ReturnUrl;
            _webhookUrl = opts.WebhookUrl;
            _premiumService = premiumService;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePayment()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user id");

            const string productName = "Gói Premium (1 tháng)";
            const int quantity = 1;
            const int price = 50000;
            var total = quantity * price;

            var items = new List<ItemData> { new ItemData(productName, quantity, price) };

            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var description = $"UserId:{userId}";

            var paymentData = new PaymentData(orderCode, total, description, items, _returnUrl, _webhookUrl);

            var result = await _payOS.createPaymentLink(paymentData);

            await _premiumService.CreatePaymentAsync(new PayosPayment
            {
                Userid = userId,
                OrderCode = orderCode,
                PaymentLinkId = result.paymentLinkId,
                Amount = total,
                Description = description,
                Status = "PENDING",
                CheckoutUrl = result.checkoutUrl,
                Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            });

            return Ok(new
            {
                orderId = orderCode,
                checkoutUrl = result.checkoutUrl,
                userId
            });
        }

        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                var payload = JsonSerializer.Deserialize<PayOSCallbackPayload>(body);

                if (payload?.data == null)
                    return Ok(new { message = "Payload không hợp lệ" });

                long orderCode = payload.data.orderCode;
                int amount = payload.data.amount;
                string description = payload.data.description;
                string paymentLinkId = payload.data.paymentLinkId;

                // Lấy payment từ DB
                var payment = await _premiumService.GetPaymentByOrderCodeAsync(orderCode);

                if (payment == null)
                {
                    // Nếu chưa có payment, tạo mới luôn để tránh lỗi Entity Framework
                    await _premiumService.CreatePaymentAsync(new PayosPayment
                    {
                        Userid = ExtractUserIdFromDescription(description), // Parse UserId từ description nếu cần
                        OrderCode = orderCode,
                        PaymentLinkId = paymentLinkId,
                        Amount = amount,
                        Description = description,
                        Status = "PAID",
                        TransactionDatetime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                        Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                        Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                    });
                }
                else
                {
                    // Cập nhật trạng thái payment
                    await _premiumService.UpdatePaymentStatusAsync(
                        orderCode,
                        "PAID",
                        DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                    );
                }

                // Nâng cấp user nếu chưa premium
                await _premiumService.UpgradeUserRoleToPremiumAsync(payment?.Userid ?? ExtractUserIdFromDescription(description));

                return Ok(new { message = "Callback xử lý thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Callback error: " + ex);
                return Ok(new { message = "Callback xảy ra lỗi", error = ex.Message });
            }
        }

        // --- Helper parse UserId từ description (ví dụ: "CSJH8XARL45 UserId10") ---
        private int ExtractUserIdFromDescription(string description)
        {
            try
            {
                var parts = description.Split(' ');
                foreach (var part in parts)
                {
                    if (part.StartsWith("UserId"))
                        return int.Parse(part.Substring(6));
                }
            }
            catch { }
            return 0;
        }

        // --- Class deserialize JSON ---
        public class PayOSCallbackPayload
        {
            public string code { get; set; }
            public string desc { get; set; }
            public PayOSData data { get; set; }
            public string signature { get; set; }
        }

        public class PayOSData
        {
            public long orderCode { get; set; }
            public int amount { get; set; }
            public string description { get; set; }
            public string paymentLinkId { get; set; }
        }
    }
}
