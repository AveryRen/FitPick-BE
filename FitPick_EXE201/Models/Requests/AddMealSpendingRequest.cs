namespace FitPick_EXE201.Models.Requests
{
    public class AddMealSpendingRequest
    {
        public int MealId { get; set; }
    }

    public class AddMealsSpendingRequest
    {
        public List<int> MealIds { get; set; } = new();
    }
}
