namespace FitPick_EXE201.Models.DTOs
{
    public class CreateMealHistoryDto
    {
        public int? Mealid { get; set; }
        public int? MealtimeId { get; set; }
        public DateOnly Date { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public int? Calories { get; set; }
    }
}
