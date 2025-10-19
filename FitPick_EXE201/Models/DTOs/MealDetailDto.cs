namespace FitPick_EXE201.Models.DTOs
{
    public class MealDetailDto
    {
        public int Mealid { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? Calories { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Carbs { get; set; }
        public decimal? Fat { get; set; }
        public int? Cookingtime { get; set; }
        public string? Diettype { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsPremium { get; set; }
        public string? CategoryName { get; set; }
        public string? StatusName { get; set; }
        
        // Chi tiết nguyên liệu
        public List<MealIngredientDetailDto>? Ingredients { get; set; }
        
        // Hướng dẫn nấu
        public List<MealInstructionDto>? Instructions { get; set; }
    }

    public class MealIngredientDetailDto
    {
        public int? IngredientId { get; set; }
        public string IngredientName { get; set; } = null!;
        public string? IngredientType { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
    }

    public class MealInstructionDto
    {
        public int? MealId { get; set; }
        public int StepNumber { get; set; }
        public string Instruction { get; set; } = null!;
    }
}
