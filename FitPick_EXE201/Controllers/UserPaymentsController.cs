using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Models.DTOs;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using FitPick_EXE201.Models.Requests;

namespace FitPick_EXE201.Controllers
{
    [Route("api/user/payments")]
    [ApiController]
    public class UserPaymentsController : ControllerBase
    {
        private readonly PayosPaymentService _paymentService;
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserPaymentsController(
            PayosPaymentService paymentService,
            UserService userService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _paymentService = paymentService;
            _userService = userService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        // POST: api/user/payments
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            // 1. Lấy userId từ JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "User not authenticated" },
                    "Unauthorized"));

            int userId = int.Parse(userIdClaim.Value);

            // 2. Lấy thông tin user từ DB
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { "User not found" },
                    "Failed"));

            // 3. Tạo payment mới
            var payment = new PayosPayment
            {
                Userid = userId,
                Amount = dto.Amount,
                Description = dto.Description,
                Status = "Pending",
                OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // 4. Tạo payload và signature
            string returnUrl = _configuration["PayOS:ReturnUrl"];
            string cancelUrl = _configuration["PayOS:CancelUrl"];
            string checksumKey = _configuration["PayOS:ChecksumKey"];

            string Encode(string s) => System.Web.HttpUtility.UrlEncode(s ?? "");
            string dataToSign = $"amount={payment.Amount}&cancelUrl={Encode(cancelUrl)}&description={Encode(payment.Description)}&orderCode={payment.OrderCode}&returnUrl={Encode(returnUrl)}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
            string signature = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign)))
                                    .Replace("-", "")
                                    .ToLower();

            var payload = new
            {
                orderCode = payment.OrderCode,
                amount = (int)payment.Amount,
                description = payment.Description,
                buyerName = user.Fullname,
                buyerEmail = user.Email,
                cancelUrl,
                returnUrl,
                signature
            };

            // 5. Gọi API PayOS merchant đúng host
            var baseUrl = _configuration["PayOS:BaseUrl"]; // live endpoint
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("x-client-idx-api-key", _configuration["PayOS:ApiKey"]);
            httpClient.DefaultRequestHeaders.Add("x-partner-code", _configuration["PayOS:PartnerCode"]);

            var response = await httpClient.PostAsync(baseUrl, jsonContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, ApiResponse<object>.ErrorResponse(
                    new List<string> { responseContent },
                    "PayOS API call failed"));

            // 6. Deserialize response PayOS
            var payosResult = JsonConvert.DeserializeObject<PayosResponse>(responseContent);
            if (payosResult?.data == null || string.IsNullOrEmpty(payosResult.data.checkoutUrl))
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    new List<string> { "PayOS response missing checkoutUrl" },
                    "Payment creation failed"));

            payment.CheckoutUrl = payosResult.data.checkoutUrl;

            // 7. Lưu payment vào DB
            var createdPayment = await _paymentService.CreatePaymentAsync(payment);

            // 8. Trả về response cho client
            return Ok(ApiResponse<object>.SuccessResponse(
                new
                {
                    payment = new
                    {
                        createdPayment.Paymentid,
                        createdPayment.Userid,
                        createdPayment.Amount,
                        createdPayment.Description,
                        createdPayment.Status,
                        createdPayment.OrderCode,
                        createdPayment.CheckoutUrl
                    },
                    payosResponse = payosResult
                },
                "Payment created successfully"
            ));
        }
    }
}
