using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> CreatePayment()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user id");

            const string productName = "Gói Premium (1 tháng)";
            const int quantity = 1;
            const int price = 50000;
            var total = quantity * price;

            var items = new List<ItemData> { new ItemData(productName, quantity, price) };

            // Sử dụng Guid để tránh trùng OrderCode
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var description = $"UserId:{userId}";

            var paymentData = new PaymentData(orderCode, total, description, items, _returnUrl, _webhookUrl);

            var result = await _payOS.createPaymentLink(paymentData);

            // Lưu PENDING, không gán PaymentId
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

        // Callback chung GET/POST
        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback()
        {
            string code = null, id = null, status = null, description = null;
            long orderCode = 0;

            // POST JSON
            if (Request.Method == "POST")
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                var payload = JsonSerializer.Deserialize<JsonElement>(body);
                var data = payload.GetProperty("data");

                status = data.GetProperty("status").GetString();
                orderCode = data.GetProperty("orderCode").GetInt64();
                description = data.GetProperty("description").GetString();
                code = payload.GetProperty("code").GetString();
                id = data.GetProperty("id").GetString();
            }
            else // GET query
            {
                status = Request.Query["status"];
                description = Request.Query["description"];
                code = Request.Query["code"];
                id = Request.Query["id"];
                long.TryParse(Request.Query["orderCode"], out orderCode);
            }

            if (string.IsNullOrEmpty(description))
                return BadRequest(new { message = "Missing description" });

            var match = System.Text.RegularExpressions.Regex.Match(description, @"UserId:(\d+)");
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out var userId))
                return BadRequest(new { message = "Invalid userId format" });

            var paidTime = DateTime.UtcNow;

            // Cập nhật transaction, check tồn tại trước
            var updated = await _premiumService.UpdatePaymentStatusAsync(orderCode, status ?? "UNKNOWN", paidTime);
            if (!updated)
                return NotFound(new { message = "OrderCode not found" });

            if (string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
                await _premiumService.UpgradeUserRoleToPremiumAsync(userId);

            return Ok(new { message = "Callback processed" });
        }
    }
}