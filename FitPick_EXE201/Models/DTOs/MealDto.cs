namespace FitPick_EXE201.Models.DTOs
{
    public class MealDto
    {
        public int Mealid { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fat { get; set; }
        public int? Cookingtime { get; set; }
        public string? Diettype { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsPremium { get; set; }

        public string? CategoryName { get; set; }
        public string? StatusName { get; set; }

        public List<string>? Instructions { get; set; }
        public List<MealIngredientDto>? Ingredients { get; set; }
    }
}