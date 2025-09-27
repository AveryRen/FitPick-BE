namespace FitPick_EXE201.Models.DTOs
{
    public class UserMealIngredientDto
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public bool HasIt { get; set; }
    }
}
