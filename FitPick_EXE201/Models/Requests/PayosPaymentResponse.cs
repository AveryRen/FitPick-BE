namespace FitPick_EXE201.Models.Requests
{
    public class PayosPaymentResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public PayosPaymentData Data { get; set; } = new PayosPaymentData();
    }
    public class PayosPaymentData
    {
        public string CheckoutUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PaymentLinkId { get; set; } = string.Empty;
    }
}
