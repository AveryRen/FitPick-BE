namespace FitPick_EXE201.Models.DTOs
{
    public class TodayMealPlanDto
    {
        public DateTime Date { get; set; }
        public string MealTime { get; set; }
        public MealDto Meal { get; set; }
    }
}
