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

        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback()
        {
            try
            {
                string code = Request.Query["code"];
                string id = Request.Query["id"];
                string status = Request.Query["status"];
                long orderCode = 0;
                long.TryParse(Request.Query["orderCode"], out orderCode);

                var payment = await _premiumService.GetPaymentByOrderCodeAsync(orderCode);
                if (payment == null)
                    return Ok(new { message = "OrderCode not found" }); // trả 200 để PayOS không fail

                int userId = payment.Userid;
                var paidTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                try { await _premiumService.UpdatePaymentStatusAsync(orderCode, status ?? "UNKNOWN", paidTime); }
                catch (Exception ex) { Console.WriteLine("UpdatePaymentStatusAsync error: " + ex); }

                if (string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    try { await _premiumService.UpgradeUserRoleToPremiumAsync(userId); }
                    catch (Exception ex) { Console.WriteLine("UpgradeUserRoleToPremiumAsync error: " + ex); }
                }

                return Ok(new { message = "Callback processed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Callback outer error: " + ex);
                return Ok(new { message = "Callback received but error occurred", error = ex.Message });
            }
        }
    }
}