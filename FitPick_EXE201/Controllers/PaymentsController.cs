using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models.Entities;
using FitPick_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPick_EXE201.Controllers
{
    [Route("api/admin/payments")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminPaymentsController : ControllerBase
    {
        private readonly PayosPaymentService _paymentService;

        public AdminPaymentsController(PayosPaymentService paymentService)
        {
            _paymentService = paymentService;
        }


        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> GetAllPayments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] int? userId = null)
        {
            if (page > 0 && pageSize > 0)
            {
                var (payments, totalCount) = await _paymentService.GetAllPaymentsPagedAsync(page, pageSize, search, status, userId);
                var result = new
                {
                    items = payments,
                    totalItems = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    pageSize = pageSize,
                    pageNumber = page
                };
                return Ok(ApiResponse<object>.SuccessResponse(result, "All payments retrieved successfully"));
            }
            else
            {
                var payments = await _paymentService.GetAllPaymentsAsync();
                return Ok(ApiResponse<IEnumerable<PayosPayment>>
                    .SuccessResponse(payments, "All payments retrieved successfully"));
            }
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PayosPayment>), 200)]
        [ProducesResponseType(typeof(ApiResponse<PayosPayment>), 404)]
        public async Task<ActionResult<ApiResponse<PayosPayment>>> GetPaymentById(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound(ApiResponse<PayosPayment>
                    .ErrorResponse(new List<string> { "Payment not found" }, "Failed"));

            return Ok(ApiResponse<PayosPayment>
                .SuccessResponse(payment, "Payment retrieved successfully"));
        }


        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PayosPayment>>), 200)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PayosPayment>>>> GetPaymentsByUser(int userId)
        {
            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<PayosPayment>>
                .SuccessResponse(payments, "Payments retrieved successfully"));
        }


        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<object>>> DeletePayment(int id)
        {
            var deleted = await _paymentService.DeletePaymentAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<object>
                    .ErrorResponse(new List<string> { "Payment not found" }, "Delete failed"));

            return Ok(ApiResponse<object>
                .SuccessResponse(null, "Payment deleted successfully"));
        }

        [HttpPut("{id:int}/status")]
        [ProducesResponseType(typeof(ApiResponse<PayosPayment>), 200)]
        [ProducesResponseType(typeof(ApiResponse<PayosPayment>), 404)]
        public async Task<ActionResult<ApiResponse<PayosPayment>>> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(ApiResponse<PayosPayment>.ErrorResponse(
                    new List<string> { "Status is required" }, "Bad Request"));
            }

            var updated = await _paymentService.UpdatePaymentStatusAsync(id, request.Status);
            if (!updated)
            {
                return NotFound(ApiResponse<PayosPayment>.ErrorResponse(
                    new List<string> { "Payment not found" }, "Update failed"));
            }

            var payment = await _paymentService.GetPaymentByIdAsync(id);
            return Ok(ApiResponse<PayosPayment>.SuccessResponse(payment!, "Payment status updated successfully"));
        }
    }

    public class UpdatePaymentStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
