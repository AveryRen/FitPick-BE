namespace FitPick_EXE201.Models.DTOs
{
    public class MealResponseDto
    {
        public int Mealid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Calories { get; set; }
        public int? Cookingtime { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Carbs { get; set; }
        public decimal? Fat { get; set; }
        public decimal? Price { get; set; }
    }

    // DTO dùng để nhận filter query params
    public class MealFilterDto
    {
        public int? CategoryId { get; set; }
        public int? MaxCalories { get; set; }
        public string? DietType { get; set; }
    }
}
