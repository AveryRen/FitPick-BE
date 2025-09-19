namespace FitPick_EXE201.Models.Requests
{
    public class PayosResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public PayosData data { get; set; }
    }

    public class PayosData
    {
        public string checkoutUrl { get; set; }
    }