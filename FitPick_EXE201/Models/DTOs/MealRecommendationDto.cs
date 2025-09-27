namespace FitPick_EXE201.Models.DTOs
{
    public class MealRecommendationDto
    {
        public int Mealid { get; set; }
        public string Name { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
        public bool IsPremium { get; set; }
    }
}
