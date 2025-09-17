namespace FitPick_EXE201.Models.DTOs
{
    public class MealHistoryResponseDto
    {
        public int Historyid { get; set; }       
        public int? Mealid { get; set; }
        public int? MealtimeId { get; set; }
        public DateOnly Date { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public int? Calories { get; set; }
    }
}
