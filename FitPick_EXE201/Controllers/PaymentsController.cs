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
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PayosPayment>>), 200)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PayosPayment>>>> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(ApiResponse<IEnumerable<PayosPayment>>
                .SuccessResponse(payments, "All payments retrieved successfully"));
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
    }
}
