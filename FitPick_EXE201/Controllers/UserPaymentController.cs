using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FitPick_EXE201.Models.Requests;
using FitPick_EXE201.Services;           // ✅ add service namespace
using FitPick_EXE201.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;

namespace FitPick_EXE201.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPaymentController : ControllerBase
    {
        private readonly PayOS _payOS;
        private readonly string _checksumKey;
        private readonly string _returnUrl;
        private readonly string _webhookUrl;
        private readonly UserPremiumService _premiumService;

        public UserPaymentController(
            IOptions<PayOSSettings> options,
            UserPremiumService premiumService  
        )
        {
            var opts = options.Value;
            _payOS = new PayOS(opts.ClientId, opts.ApiKey, opts.ChecksumKey);
            _checksumKey = opts.ChecksumKey;
            _returnUrl = opts.ReturnUrl;
            _webhookUrl = opts.WebhookUrl;
            _premiumService = premiumService;
        }

        /// <summary>
        /// Tạo link thanh toán PayOS (mua 1 gói Premium/tháng)
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment()
        {
            // ✅ Lấy userId từ JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user id");

            const string productName = "Gói Premium (1 tháng)";
            const int quantity = 1;
            const int price = 50000;

            var item = new ItemData(productName, quantity, price);
            var items = new List<ItemData> { item };

            var orderID = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var total = quantity * price;

            var description = $"Premium: {userId}";

            var paymentData = new PaymentData(
                orderID,
                total,
                description,
                items,
                _returnUrl,
                _webhookUrl
            );

            var result = await _payOS.createPaymentLink(paymentData);

            return Ok(new
            {
                orderId = orderID,
                checkoutUrl = result.checkoutUrl,
                userId = userId
            });
        }


        /// <summary>
        /// Webhook PayOS callback kết quả thanh toán
        /// </summary>
        [HttpPost("callback")]
        public async Task<IActionResult> Callback()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var signature = Request.Headers["X-PAYOS-SIGNATURE"].FirstOrDefault();

            if (!VerifySignature(body, signature))
                return Unauthorized("Invalid signature");

            try
            {
                var payload = JsonSerializer.Deserialize<JsonElement>(body);
                var data = payload.GetProperty("data");
                var status = data.GetProperty("status").GetString();
                var description = data.GetProperty("description").GetString();

                if (string.IsNullOrEmpty(description))
                    return BadRequest(new { message = "Missing description" });

                // ✅ Tìm userId trong chuỗi mô tả
                var match = System.Text.RegularExpressions.Regex.Match(description, @"UserId:(\d+)");
                if (!match.Success || !int.TryParse(match.Groups[1].Value, out var userId))
                    return BadRequest(new { message = "Invalid userId format" });

                // ✅ Chỉ nâng cấp khi trạng thái là PAID
                if (string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    var ok = await _premiumService.UpgradeUserRoleToPremiumAsync(userId);
                    if (!ok)
                        return NotFound(new { message = "User not found to upgrade" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid JSON payload", error = ex.Message });
            }

            return Ok(new { message = "Callback processed" });
        }

        private bool VerifySignature(string body, string? signature)
        {
            if (string.IsNullOrEmpty(signature)) return false;

            var key = Encoding.UTF8.GetBytes(_checksumKey);
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            var expected = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            return expected == signature.ToLowerInvariant();
        }
    }
}
