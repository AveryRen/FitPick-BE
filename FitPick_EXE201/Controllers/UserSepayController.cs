using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace FitPick_EXE201.Controllers
{
    [Route("api/user/sepay")]
    [ApiController]
    public class UserSepayController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public UserSepayController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("create-va-mock")]
        public async Task<IActionResult> CreateVAMock([FromBody] CreateVADto dto)
        {
            try
            {
                // 1. Lấy thông tin cấu hình sandbox
                var clientId = _configuration["SePay:ClientId"];
                var apiKey = _configuration["SePay:ApiKey"];
                var sandboxUrl = _configuration["SePay:SandboxUrl"]; // ví dụ: https://sandbox.sepay.vn/api/va

                // 2. Tạo payload cho request
                var payload = new
                {
                    amount = dto.Amount,
                    description = dto.Description,
                    customerName = dto.CustomerName ?? "Test User",
                    customerEmail = dto.CustomerEmail ?? "test@example.com"
                };

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("x-client-id", clientId);
                httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                // 3. Gửi POST request đến SePay sandbox
                var response = await httpClient.PostAsync(sandboxUrl, jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        success = false,
                        message = "SePay API call failed",
                        error = responseContent
                    });
                }

                // 4. Deserialize response
                var vaResult = JsonConvert.DeserializeObject<dynamic>(responseContent);

                // 5. Trả về kết quả cho frontend
                return Ok(new
                {
                    success = true,
                    message = "VA created successfully",
                    data = vaResult
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }

    // DTO đơn giản
    public class CreateVADto
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
