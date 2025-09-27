namespace FitPick_EXE201.Models.DTOs
{
    public class ProgressDto
    {
        public decimal? CurrentWeight { get; set; }
        public decimal? TargetWeight { get; set; }
        public double WeightProgress => TargetWeight.HasValue && TargetWeight.Value > 0
            ? Math.Round((double)((CurrentWeight ?? 0) - TargetWeight.Value) / (double)TargetWeight.Value * 100, 1)
            : 0;

        public double CurrentCalories { get; set; }
        public int? TargetCalories { get; set; }
        public double CaloriesProgress => TargetCalories.HasValue && TargetCalories.Value > 0
            ? Math.Round(CurrentCalories / TargetCalories.Value * 100, 1)
            : 0;
    }
}
