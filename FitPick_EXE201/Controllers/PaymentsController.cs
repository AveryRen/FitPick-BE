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

        // GET: api/admin/payments
        // Lấy tất cả giao dịch (dành cho admin)
        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(ApiResponse<IEnumerable<PayosPayment>>.SuccessResponse(payments, "All payments retrieved successfully"));
        }

        // GET: api/admin/payments/{id} 
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound(ApiResponse<PayosPayment>.ErrorResponse(new List<string> { "Payment not found" }, "Failed"));

            return Ok(ApiResponse<PayosPayment>.SuccessResponse(payment, "Payment retrieved successfully"));
        }

        // GET: api/admin/payments/user/{userId}
        // Lấy tất cả giao dịch của 1 user
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetPaymentsByUser(int userId)
        {
            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<PayosPayment>>.SuccessResponse(payments, "Payments retrieved successfully"));
        }

        // DELETE: api/admin/payments/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var deleted = await _paymentService.DeletePaymentAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { "Payment not found" }, "Delete failed"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Payment deleted successfully"));
        }
    }
}
