namespace FitPick_EXE201.Models.DTOs
{
    public class NutritionStatsDto
    {
        public int TargetCalories { get; set; }
        public double ConsumedCalories { get; set; }
        public MacroNutrientDto Starch { get; set; } = new MacroNutrientDto();
        public MacroNutrientDto Protein { get; set; } = new MacroNutrientDto();
        public MacroNutrientDto Fat { get; set; } = new MacroNutrientDto();
    }

    public class MacroNutrientDto
    {
        public double Current { get; set; }
        public double Target { get; set; }
    }
}
