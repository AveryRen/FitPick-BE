namespace FitPick_EXE201.Models.DTOs
{
    public class CreatePaymentDto
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }
}
