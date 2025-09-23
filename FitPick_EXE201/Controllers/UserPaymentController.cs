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
            var description = $"{userId}";

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

                // 🔑 Lấy userId từ description (VD: "CSDN5AQ5B25 35")
                var userId = ExtractUserIdFromDescription(payload.data.description);
                if (userId <= 0)
                    return Ok(new { message = "Không lấy được userId từ description" });

                // ⚡️ Parse transactionDateTime thủ công
                DateTime? transTime = null;
                if (!string.IsNullOrWhiteSpace(payload.data.transactionDateTime))
                {
                    // PayOS format: "yyyy-MM-dd HH:mm:ss"
                    if (DateTime.TryParseExact(
                        payload.data.transactionDateTime,
                        "yyyy-MM-dd HH:mm:ss",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsed))
                    {
                        transTime = parsed;
                    }
                }

                // ⚡️ Nâng cấp User lên Premium
                await _premiumService.UpgradeUserRoleToPremiumAsync(userId);

                // ⚡️ Cập nhật trạng thái giao dịch
                await _premiumService.UpdatePaymentStatusAsync(
                    orderCode: payload.data.orderCode,
                    status: "PAID",
                    transactionTime: transTime,
                    amount: payload.data.amount,
                    description: payload.data.description
                );

                return Ok(new
                {
                    message = $"✅ User {userId} đã được nâng cấp Premium và cập nhật giao dịch thành công"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Callback error: " + ex);
                return Ok(new { message = "❌ Lỗi khi xử lý callback", error = ex.Message });
            }
        }

        private int ExtractUserIdFromDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return 0;

            var parts = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var lastPart = parts.LastOrDefault();

            return int.TryParse(lastPart, out var userId) ? userId : 0;
        }

        // --- Class deserialize JSON ---
        public class PayOSCallbackPayload
        {
            public string code { get; set; }
            public string desc { get; set; }
            public bool success { get; set; }
            public PayOSData data { get; set; }
            public string signature { get; set; }
        }

        public class PayOSData
        {
            public string accountNumber { get; set; }
            public decimal amount { get; set; }
            public string description { get; set; }
            public string reference { get; set; }
            public string transactionDateTime { get; set; }   // ⚡ string để tự parse
            public string virtualAccountNumber { get; set; }
            public string counterAccountBankId { get; set; }
            public string counterAccountBankName { get; set; }
            public string counterAccountName { get; set; }
            public string counterAccountNumber { get; set; }
            public string virtualAccountName { get; set; }
            public string currency { get; set; }
            public long orderCode { get; set; }
            public string paymentLinkId { get; set; }
            public string code { get; set; }
            public string desc { get; set; }
        }
    }
}