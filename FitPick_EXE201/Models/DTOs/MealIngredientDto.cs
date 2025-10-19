namespace FitPick_EXE201.Models.DTOs
{
    public class MealIngredientDto
    {
        public int IngredientId { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public bool HasIt { get; set; } // user dã có nguyên li?u này
    }
}
