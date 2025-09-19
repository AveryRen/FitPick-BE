namespace FitPick_EXE201.Models.DTOs
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public string BuyerName { get; set; } = null!;
        public string BuyerPhone { get; set; } = null!;
        public string CancelUrl { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
        public long ExpiredAt { get; set; }
    }
}
